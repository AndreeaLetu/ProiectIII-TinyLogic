
const PYTHON_BASE = "http://localhost:5000";
const POLL_INTERVAL = 200;

let polling = null;
let lastGesture = "NONE";
let cooldown = false;


async function activateGestures() {
    await fetch("http://localhost:5000/start", { method: "POST" });
    alert("Control total ACTIV");
}

async function stopGestures() {
    await fetch("http://localhost:5000/stop", { method: "POST" });
    alert("Control total OPRIT");
}


function openGestureBox() {
    const box = document.getElementById("gestureBox");
    const video = document.getElementById("gestureVideo");

    box.classList.remove("hidden");
    video.src = `${PYTHON_BASE}/video-feed`;
}

function closeGestureBox() {
    const box = document.getElementById("gestureBox");
    const video = document.getElementById("gestureVideo");

    box.classList.add("hidden");
    video.src = "";

    stopGesturePolling();
}


function startGesturePolling() {
    if (polling) return;

    polling = setInterval(async () => {
        try {
            const res = await fetch(`${PYTHON_BASE}/gesture`);
            const data = await res.json();

            const gesture = data.gesture || "NONE";
            document.getElementById("gestureLabel").innerText = gesture;

            handleGesture(gesture);
        } catch (e) {
            console.warn("Gesture poll error");
        }
    }, POLL_INTERVAL);
}

function stopGesturePolling() {
    if (polling) {
        clearInterval(polling);
        polling = null;
    }
}

function handleGesture(gesture) {
    if (gesture === lastGesture || cooldown) return;

    lastGesture = gesture;
    cooldown = true;
    setTimeout(() => cooldown = false, 600);

    switch (gesture) {
        case "PINCH":
            gestureClick();
            break;

        case "MOVE":
            gestureScroll();
            break;

        case "PAUSE":
            gesturePause();
            break;
    }
}


function gestureClick() {
    const el = document.querySelector(".gesture-target");
    if (!el) return;

    el.click();

    el.classList.add("ring-4", "ring-green-400");
    setTimeout(() => {
        el.classList.remove("ring-4", "ring-green-400");
    }, 300);

    console.log("PINCH → CLICK");
}


function gestureScroll() {
    window.scrollBy({
        top: 200,
        behavior: "smooth"
    });
    console.log("MOVE → SCROLL");
}

function gesturePause() {
    document.body.style.pointerEvents = "none";
    document.body.style.opacity = "0.6";

    console.log("PAUSE → PAGE LOCK");

    setTimeout(() => {
        document.body.style.pointerEvents = "auto";
        document.body.style.opacity = "1";
    }, 1500);
}


document.addEventListener("DOMContentLoaded", () => {
    const box = document.getElementById("gestureBox");
    if (!box) return;

    let offsetX = 0, offsetY = 0, dragging = false;

    box.addEventListener("mousedown", (e) => {
        dragging = true;
        offsetX = e.clientX - box.offsetLeft;
        offsetY = e.clientY - box.offsetTop;
    });

    document.addEventListener("mousemove", (e) => {
        if (!dragging) return;
        box.style.left = `${e.clientX - offsetX}px`;
        box.style.top = `${e.clientY - offsetY}px`;
    });

    document.addEventListener("mouseup", () => dragging = false);
});
