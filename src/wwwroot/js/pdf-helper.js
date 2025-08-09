window.downloadLearnerListPDF = () => {
    const element = document.getElementById("learner-list");
    if (!element) {
        console.error("LearnerList element not found.");
        return;
    }

    html2canvas(element).then(canvas => {
        const imgData = canvas.toDataURL('image/png');
        const { jsPDF } = window.jspdf;

        const pdf = new jsPDF('p', 'pt', 'a4');
        const margin = 20;
        const pdfWidth = pdf.internal.pageSize.getWidth() - margin * 2;
        const pdfHeight = (canvas.height * pdfWidth) / canvas.width;

        pdf.addImage(imgData, 'PNG', margin, margin, pdfWidth, pdfHeight);
        pdf.save("LearnerList.pdf");
    });
}

window.downloadFileFromBytes = (filename, contentType, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    setTimeout(() => URL.revokeObjectURL(url), 1000);
};