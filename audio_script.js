let audioCtx = null;
function initAudio() {
  if (!audioCtx) {
    audioCtx = new (window.AudioContext || window.webkitAudioContext)();
  }
  if (audioCtx.state === 'suspended') {
    audioCtx.resume();
  }
}

function playSynthesizedSound(soundName) {
  try {
    initAudio();
    const osc1 = audioCtx.createOscillator();
    const osc2 = audioCtx.createOscillator();
    const gainNode = audioCtx.createGain();
    const now = audioCtx.currentTime;

    osc1.connect(gainNode);
    osc2.connect(gainNode);
    gainNode.connect(audioCtx.destination);

    if (soundName === 'success') {
      osc1.type = 'sine'; osc2.type = 'triangle';
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(0.3, now + 0.05);
      gainNode.gain.setValueAtTime(0.3, now + 0.4);
      gainNode.gain.exponentialRampToValueAtTime(0.01, now + 1.0);
      [523.25, 659.25, 783.99, 1046.50].forEach((freq, i) => {
        osc1.frequency.setValueAtTime(freq, now + i * 0.15);
        osc2.frequency.setValueAtTime(freq, now + i * 0.15);
      });
      osc1.start(now); osc2.start(now);
      osc1.stop(now + 1.0); osc2.stop(now + 1.0);
    } else if (soundName === 'timeout') {
      osc1.type = 'sawtooth';
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(0.3, now + 0.05);
      gainNode.gain.exponentialRampToValueAtTime(0.01, now + 1.0);
      osc1.frequency.setValueAtTime(300, now);
      osc1.frequency.exponentialRampToValueAtTime(50, now + 1.0);
      osc1.start(now); osc1.stop(now + 1.0);
    } else if (soundName === 'active') {
      osc1.type = 'square';
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(0.1, now + 0.02);
      gainNode.gain.exponentialRampToValueAtTime(0.01, now + 0.2);
      osc1.frequency.setValueAtTime(880, now);
      osc1.start(now); osc1.stop(now + 0.2);
    } else if (soundName === 'recap') {
      osc1.type = 'sine'; osc2.type = 'sine';
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(0.2, now + 0.05);
      gainNode.gain.exponentialRampToValueAtTime(0.01, now + 1.5);
      osc1.frequency.setValueAtTime(440, now);
      osc2.frequency.setValueAtTime(554.37, now);
      osc1.start(now); osc2.start(now);
      osc1.stop(now + 1.5); osc2.stop(now + 1.5);
    } else if (soundName === 'waiting') {
      osc1.type = 'sine';
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(0.2, now + 0.02);
      gainNode.gain.setValueAtTime(0.2, now + 0.1);
      gainNode.gain.linearRampToValueAtTime(0, now + 0.12);
      gainNode.gain.setValueAtTime(0, now + 0.2);
      gainNode.gain.linearRampToValueAtTime(0.2, now + 0.22);
      gainNode.gain.setValueAtTime(0.2, now + 0.3);
      gainNode.gain.linearRampToValueAtTime(0, now + 0.32);
      osc1.frequency.setValueAtTime(600, now);
      osc1.start(now); osc1.stop(now + 0.35);
    } else if (soundName === 'inactive') {
      osc1.type = 'sine';
      gainNode.gain.setValueAtTime(0, now);
      gainNode.gain.linearRampToValueAtTime(0.2, now + 0.05);
      gainNode.gain.exponentialRampToValueAtTime(0.01, now + 0.5);
      osc1.frequency.setValueAtTime(200, now);
      osc1.start(now); osc1.stop(now + 0.5);
    }
  } catch(e) {
    console.warn("Audio play error", e);
  }
}

function playSound(soundName) {
  const audio = new Audio(`${soundName}.mp3`);
  audio.oncanplaythrough = () => {
      audio.play().catch(e => console.log("play error", e));
  };
  audio.onerror = () => {
      playSynthesizedSound(soundName);
  };
}
