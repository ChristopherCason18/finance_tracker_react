import { useState, useImperativeHandle } from 'react';
import axios from 'axios';
import './App.css';

export default function FileUploader(_, ref) {
  const [file, setFile] = useState(null);
  const [status, setStatus] = useState('idle');
  const [uploadProgress, setUploadProgress] = useState(0);

  function handleFileChange(e) {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    }
  }

  async function handleFileUpload() {
    if (!file) return;

    setStatus('uploading');
    setUploadProgress(0);

    const formData = new FormData();
    formData.append('file', file);

    try {
      // Acquire antiforgery token first
      const tokenResp = await axios.get('/antiforgery/token', { withCredentials: true });
      const token = tokenResp.data?.token;

      const res = await axios.post('/transactions/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data', 'RequestVerificationToken': token },
        withCredentials: true,
        onUploadProgress: (e) => {
          const progress = e.total ? Math.round((e.loaded * 100) / e.total) : 0;
          setUploadProgress(progress);
        },
      });

      console.log('Upload response', res.data);
      setStatus('success');
      // Dispatch an event so App.jsx can refresh the transactions table
        window.dispatchEvent(new Event('transactions:uploaded'));
        } catch (err) {
          setStatus('error');
          console.error(err);
        }
  }
  // expose upload method to parent via ref
  useImperativeHandle(ref, () => ({ upload: handleFileUpload }));
  return (
    <div className="space-y-2">
      <input type="file" onChange={handleFileChange} />

      {file && (
        <div className="mb-4 text-sm">
          <p>File name: {file.name}</p>
          <p>Size: {(file.size / 1024).toFixed(2)} KB</p>
          <p>Type: {file.type}</p>
        </div>
      )}

      {status === 'uploading' && (
        <div className="space-y-2">
          <div className="h-2.5 w-full rounded-full bg-gray-200">
            <div
              className="h-2.5 rounded-full bg-blue-600 transition-all duration-300"
              style={{ width: `${uploadProgress}%` }}
            />
          </div>
          <p className="text-sm text-gray-600">{uploadProgress}% uploaded</p>
        </div>
      )}

      {file && status !== 'uploading' && <button onClick={handleFileUpload}>Upload</button>}

      {status === 'success' && <p className="text-sm text-green-600">File uploaded successfully!</p>}

      {status === 'error' && <p className="text-sm text-red-600">Upload failed. Please try again.</p>}
    </div>
  );
}
