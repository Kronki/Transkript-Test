function uploadFileToVimeo() {
    var formData = new FormData();
    var inputFile = document.getElementById('fileInputVimeo');
    formData.append('videoFile', inputFile.files[0]);
    formData.append('videoName', 'Video test');
    fetch('/Home/UploadToVimeo', {
        method: "POST",
        body: formData
    })
}