//import TimelinePlugin from 'wavesurfer.js/dist/plugins/timeline.esm.js'

const wavesurfer = WaveSurfer.create({
    container: '#waveform',
    waveColor: '#4d4d4d',
    progressColor: '#000000',
    url: 'http://localhost:5157/Videos/Test.mp4',
    minPxPerSec: 200,
    //plugins: [TimelinePlugin.create()],
})

wavesurfer.once('decode', () => {
    //const slider = document.querySelector('input[type="range"]')

    //slider.addEventListener('input', (e) => {
    //    const minPxPerSec = e.target.valueAsNumber
    //    console.log(minPxPerSec)
    //    wavesurfer.zoom(minPxPerSec)
    //})
    
    var playPauseContainer = document.querySelector('.playPauseContainer');

    playPauseContainer.addEventListener('click', (event) => {
        wavesurfer.playPause();
    });

    wavesurfer.on('pause', () => {
        playPauseContainer.children[0].classList.remove('d-none');
        playPauseContainer.children[1].classList.add('d-none');
    });

    wavesurfer.on('play', () => {
        playPauseContainer.children[0].classList.add('d-none');
        playPauseContainer.children[1].classList.remove('d-none');
    })
    const subtitleUrl = 'http://localhost:5157/Subtitles';
    loadSubtitles(subtitleUrl + "/Sub1.vtt");

    wavesurfer.on('timeupdate', (currentTime) => {
        const currentCue = cues.find(cue => currentTime >= cue.start && currentTime <= cue.end);

        if (currentCue) {
            subtitleDisplay.classList.add('subtitleDisplay');
            subtitleDisplay.innerText = currentCue.text;
        } else {
            subtitleDisplay.classList.remove('subtitleDisplay');
            subtitleDisplay.innerText = '';
        }
        console.log('Time', currentTime + 's')
    })
    
})