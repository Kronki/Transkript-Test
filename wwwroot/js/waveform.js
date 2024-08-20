const wavesurfer = WaveSurfer.create({
    container: '#waveform',
    waveColor: '#666666',
    progressColor: '#000000',
    url: 'http://localhost:5157/Videos/Test.mp4',
    minPxPerSec: 200,
    barWidth: 3,
    barGap: 3,
    barHeight: 0.5,
    barMinHeight: 1,
    hideScrollbar: true
});

wavesurfer.once('decode', () => {
    const playPauseContainer = document.querySelector('.playPauseContainer');
    const startTimeElement = document.getElementById('startTime');
    const endTimeElement = document.getElementById('endTime');

    const volumeSlider = document.getElementById('volumeSlider');
    volumeSlider.addEventListener('input', (e) => {
        const volume = e.target.valueAsNumber;
        wavesurfer.setVolume(volume);
    });



    // Update the end time to the total duration
    endTimeElement.innerText = formatTime(wavesurfer.getDuration());

    playPauseContainer.addEventListener('click', () => {
        wavesurfer.playPause();
    });

    wavesurfer.on('pause', () => {
        playPauseContainer.children[0].classList.remove('d-none');
        playPauseContainer.children[1].classList.add('d-none');
    });

    wavesurfer.on('play', () => {
        playPauseContainer.children[0].classList.add('d-none');
        playPauseContainer.children[1].classList.remove('d-none');
    });

    wavesurfer.on('timeupdate', (currentTime) => {
        startTimeElement.innerText = formatTime(currentTime);
        const currentCue = cues.find(cue => currentTime >= cue.start && currentTime <= cue.end);

        if (currentCue) {
            subtitleDisplay.classList.add('subtitleDisplay');
            subtitleDisplay.innerText = currentCue.text;
        } else {
            subtitleDisplay.classList.remove('subtitleDisplay');
            subtitleDisplay.innerText = '';
        }
        console.log('Time', currentTime + 's');
    });
});

// Utility function to format time in mm:ss format
function formatTime(seconds) {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const sec = Math.floor(seconds % 60);

    if (hours > 0) {
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${sec.toString().padStart(2, '0')}`;
    } else {
        return `${minutes.toString().padStart(2, '0')}:${sec.toString().padStart(2, '0')}`;
    }
}

