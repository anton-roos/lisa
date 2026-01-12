window.downloadFile = function (filename, contentType, base64data) {
    const binary = atob(base64data);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    
    const blob = new Blob([bytes], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    
    // Create and click download link
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    
    // Cleanup
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
};