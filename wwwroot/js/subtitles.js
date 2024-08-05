function addSubtitleSection() {
    var subtitleArea = document.getElementById('subtitle-area');
    var oldButton = document.getElementById('add-section');

    var newDiv = document.createElement('div');
    newDiv.classList.add('mt-3', 'subtitle-editor');

    var newArea = document.createElement('textarea');
    newArea.setAttribute('rows', 3);
    newArea.setAttribute('cols', 49);
    newArea.classList.add('subtitle-text');

    var newTimeInputsDiv = document.createElement('div');
    newTimeInputsDiv.classList.add('time-inputs');

    var timeInputs = document.getElementsByClassName('time-inputs');
    var lastInputs = Array.from(timeInputs).at(-1);
    var firstInput = lastInputs.children[0];
    var lastInput = lastInputs.children[1];

    if (timeToSeconds(firstInput.value) > timeToSeconds(lastInput.value)) {
        return;
    }
    oldButton.remove();
    var newInputFirst = document.createElement('input');
    newInputFirst.value = lastInput.value ? lastInput.value : firstInput.value;
    newInputFirst.setAttribute('placeholder', lastInput.value ? lastInput.value : firstInput.value);
    var newInputSecond = document.createElement('input');
    newInputSecond.value = lastInput.value ? lastInput.value : firstInput.value;
    newInputSecond.setAttribute('placeholder', lastInput.value ? lastInput.value : firstInput.value);

    newInputFirst.onchange = function (event) {
        validateTime(event);
    };
    newInputSecond.onchange = function (event) {
        validateTime(event);
    };

    newTimeInputsDiv.appendChild(newInputFirst);
    newTimeInputsDiv.appendChild(newInputSecond);

    var newButton = document.createElement('button');
    newButton.id = 'add-section';
    newButton.onclick = addSubtitleSection;
    newButton.innerText = "Shto";

    newDiv.appendChild(newArea);
    newDiv.appendChild(newTimeInputsDiv);
    newDiv.appendChild(newButton);



    subtitleArea.appendChild(newDiv);
}


const removeLast = () => {
    var subtitles = document.getElementsByClassName('subtitle-editor');
    var lastSubtitle = Array.from(subtitles).at(-1);
    lastSubtitle.remove();

    var newButton = document.createElement('button');
    newButton.id = 'add-section';
    newButton.onclick = addSubtitleSection;
    newButton.innerText = "Shto";

    var lastSubtitle = Array.from(subtitles).at(-1);
    lastSubtitle.appendChild(newButton);
};


const validateTime = (event) => {
    const timePattern = /^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$/;
    if (!timePattern.test(event.target.value)) {
        event.target.value = "00:00:00";
    }
};

const timeToSeconds = (time) => {
    const [hours, minutes, seconds] = time.split(':').map(Number);
    return hours * 3600 + minutes * 60 + seconds;
};