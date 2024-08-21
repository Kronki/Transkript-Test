var fileInput = document.getElementById("fileInput");

async function uploadChunk(file, chunk, start, end, chunkNumber, totalChunks) {
    const formData = new FormData();
    formData.append('file', chunk, file.name); // Ensure 'file' matches server-side parameter
    formData.append('start', start);
    formData.append('end', end);
    formData.append('chunkNumber', chunkNumber);
    formData.append('totalChunks', totalChunks);

    try {
        const response = await fetch('http://localhost:5157/Home/AddVideo', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error(`Upload failed: ${response.statusText} - ${errorText}`);
            throw new Error(`Upload failed: ${response.statusText}`);
        }

        // Update progress
        const progress = Math.round((chunkNumber / totalChunks) * 100);
        document.getElementById('progressText').textContent = `${progress}%`;
    } catch (error) {
        console.error(`Error: ${error.message}`);
        // Handle the error as needed (e.g., show a user-friendly message)
    }
}

// Call this function to start the upload process and show the spinner
async function startUpload(file) {
    const chunkSize = 5 * 1024 * 1024; // 5MB per chunk
    const totalChunks = Math.ceil(file.size / chunkSize);

    // Show the spinner
    document.getElementById('uploadProgress').style.display = 'block';

    for (let i = 0; i < totalChunks; i++) {
        const start = i * chunkSize;
        const end = Math.min(start + chunkSize, file.size);
        const chunk = file.slice(start, end);

        await uploadChunk(file, chunk, start, end, i + 1, totalChunks);
    }

    // Hide the spinner when done
    document.getElementById('uploadProgress').style.display = 'none';
    alert('Upload complete! Conversion to MP3 started.');
}

// Assuming you have an input element to select the file
function uploadFile() {
    const file = fileInput.files[0];
    if (file) {
        startUpload(file);
    }
}
