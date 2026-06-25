const API = {
    async newGame() {
        const res = await fetch('/api/games', { method: 'POST' });
        return res.json();
    },
    async guess(id, word) {
        const res = await fetch(`/api/games/${id}/guesses`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ word })
        });
        return res.json();
    }
};

// `gen` identifies the current game. Async work (guess request, reveal animation)
// captures the gen it started with and bails out if a new game has begun since,
// so a finishing previous game can never corrupt a freshly started one.
const state = { id: null, wordLength: 5, maxAttempts: 6, row: 0, guess: '', over: false, animating: false, gen: 0 };

const boardEl = document.getElementById('board');
const keyboardEl = document.getElementById('keyboard');
const messageEl = document.getElementById('message');

const KEY_ROWS = ['AZERTYUIOP', 'QSDFGHJKLM', ['ENTER', 'W', 'X', 'C', 'V', 'B', 'N', 'BACK']];
const REASONS = {
    WrongLength: 'Il faut 5 lettres',
    NotAlphabetic: 'Lettres uniquement',
    NotInDictionary: 'Mot pas dans la liste',
    GameOver: 'La partie est terminée'
};
const RANK = { absent: 0, misplaced: 1, correct: 2 };
const REVEAL_STAGGER = 300; // ms between each letter flip

function buildBoard() {
    boardEl.innerHTML = '';
    for (let r = 0; r < state.maxAttempts; r++) {
        const row = document.createElement('div');
        row.className = 'row';
        row.dataset.row = r;
        for (let c = 0; c < state.wordLength; c++) {
            row.appendChild(document.createElement('div')).className = 'tile';
        }
        boardEl.appendChild(row);
    }
}

function buildKeyboard() {
    keyboardEl.innerHTML = '';
    for (const keys of KEY_ROWS) {
        const rowEl = document.createElement('div');
        rowEl.className = 'keyboard-row';
        for (const k of Array.isArray(keys) ? keys : keys.split('')) {
            const key = document.createElement('button');
            key.type = 'button';
            key.className = 'key' + (k === 'ENTER' || k === 'BACK' ? ' wide' : '');
            key.textContent = k === 'ENTER' ? 'Entrée' : k === 'BACK' ? '⌫' : k;
            key.dataset.key = k;
            // Blur after click so a later physical Enter/Space doesn't re-activate this focused button.
            key.addEventListener('click', () => { key.blur(); handleKey(k); });
            rowEl.appendChild(key);
        }
        keyboardEl.appendChild(rowEl);
    }
}

const currentRowEl = () => boardEl.querySelector(`.row[data-row="${state.row}"]`);

function renderGuess() {
    const tiles = currentRowEl().children;
    for (let i = 0; i < state.wordLength; i++) {
        const ch = state.guess[i] || '';
        tiles[i].textContent = ch;
        tiles[i].classList.toggle('filled', ch !== '');
    }
}

function showMessage(text, persist = false) {
    messageEl.textContent = text;
    messageEl.classList.remove('show');
    void messageEl.offsetWidth; // restart the pop animation
    messageEl.classList.add('show');
    clearTimeout(showMessage._t);
    if (!persist) {
        showMessage._t = setTimeout(() => {
            messageEl.classList.remove('show');
            messageEl.textContent = '';
        }, 1800);
    }
}

function shakeRow() {
    const row = currentRowEl();
    row.classList.add('shake');
    setTimeout(() => row.classList.remove('shake'), 400);
}

function handleKey(k) {
    if (state.over || state.animating) return;
    if (k === 'ENTER') return submitGuess();
    if (k === 'BACK') {
        state.guess = state.guess.slice(0, -1);
        return renderGuess();
    }
    if (/^[A-Z]$/.test(k) && state.guess.length < state.wordLength) {
        state.guess += k;
        renderGuess();
        const tile = currentRowEl().children[state.guess.length - 1];
        tile.classList.remove('pop');
        void tile.offsetWidth;
        tile.classList.add('pop');
    }
}

async function submitGuess() {
    if (state.animating) return;
    if (state.guess.length < state.wordLength) {
        showMessage('Pas assez de lettres');
        shakeRow();
        return;
    }

    const myGen = state.gen;
    state.animating = true;
    let res;
    try {
        res = await API.guess(state.id, state.guess);
    } catch {
        if (myGen === state.gen) {
            showMessage('Erreur réseau');
            state.animating = false;
        }
        return;
    }

    if (myGen !== state.gen) return; // a new game started during the request

    if (!res.accepted) {
        showMessage(REASONS[res.reason] || 'Proposition refusée');
        shakeRow();
        state.animating = false;
        return;
    }

    await revealRow(res.feedback);
    if (myGen !== state.gen) return; // a new game started during the reveal animation

    updateKeyboard(state.guess, res.feedback);

    const rowEl = currentRowEl();
    state.row += 1;
    state.guess = '';
    state.animating = false;

    if (res.status === 'Won') {
        state.over = true;
        celebrate(rowEl);
        showMessage('Bravo ! 🎉', true);
    } else if (res.status === 'Lost') {
        state.over = true;
        sink(rowEl);
        showMessage(`Perdu — le mot était ${res.secret}`, true);
    }
}

// Flip each tile in turn; the colour is revealed at the midpoint of the flip.
function revealRow(feedback) {
    return new Promise(resolve => {
        const tiles = currentRowEl().children;
        feedback.forEach((f, i) => {
            setTimeout(() => {
                tiles[i].classList.add('revealing');
                setTimeout(() => tiles[i].classList.add(f), 260);
            }, i * REVEAL_STAGGER);
        });
        setTimeout(resolve, (feedback.length - 1) * REVEAL_STAGGER + 520);
    });
}

function updateKeyboard(guess, feedback) {
    for (let i = 0; i < guess.length; i++) {
        const key = keyboardEl.querySelector(`.key[data-key="${guess[i]}"]`);
        if (!key) continue;
        const current = ['absent', 'misplaced', 'correct'].find(c => key.classList.contains(c));
        if (!current || RANK[feedback[i]] > RANK[current]) {
            key.classList.remove('absent', 'misplaced', 'correct');
            key.classList.add(feedback[i]);
        }
    }
}

function celebrate(rowEl) {
    [...rowEl.children].forEach((tile, i) => setTimeout(() => tile.classList.add('win'), i * 100));
}

function sink(rowEl) {
    [...rowEl.children].forEach((tile, i) => setTimeout(() => tile.classList.add('lose'), i * 80));
}

async function startGame() {
    const myGen = ++state.gen; // invalidate any in-flight async work from the previous game
    const game = await API.newGame();
    if (myGen !== state.gen) return; // another "new game" superseded this one
    state.id = game.id;
    state.wordLength = game.wordLength;
    state.maxAttempts = game.maxAttempts;
    state.row = 0;
    state.guess = '';
    state.over = false;
    state.animating = false;
    messageEl.classList.remove('show');
    messageEl.textContent = '';
    buildBoard();
    buildKeyboard();
}

document.addEventListener('keydown', (e) => {
    if (e.ctrlKey || e.metaKey || e.altKey) return;
    if (e.key === 'Enter') handleKey('ENTER');
    else if (e.key === 'Backspace') handleKey('BACK');
    else {
        const k = e.key.toUpperCase();
        if (/^[A-Z]$/.test(k)) handleKey(k);
    }
});

document.getElementById('new-game').addEventListener('click', (event) => {
    // Drop focus so pressing Enter to submit the first guess doesn't re-trigger
    // this button (which would restart the game and wipe the just-typed word).
    event.currentTarget.blur();
    startGame();
});
startGame();
