import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from "../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = `${environment.apiUrl}/Form`;

  constructor(private http: HttpClient) { }

  sendMessage(prompt: string, files?: File[]): Observable<string> {
    const formData = new FormData();
    formData.append('customPrompt', prompt);
  
    if (files && files.length > 0) {
         // Append all files to the form data
        files.forEach((file) => {
          formData.append('files', file);
        });
    }
  
    return new Observable<string>((observer) => {
      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${this.apiUrl}/chathistory?sessionId=1nks1`, true);
      xhr.setRequestHeader('Accept', 'text/plain'); // Expect a plain text streaming response
  
      let lastPosition = 0;
  
      // Process each chunk as it's received
      xhr.onprogress = () => {
        const currentText = xhr.responseText;
        const newChunk = currentText.substring(lastPosition);
        lastPosition = currentText.length; // Update position
        observer.next(newChunk); // Emit the new chunk
      };
  
      xhr.onload = () => {
        observer.complete(); // Mark as complete
      };
  
      xhr.onerror = () => {
        observer.error('An error occurred during streaming');
      };
  
      xhr.send(formData); // Send the form data
    });
  }

  getStreamingResponse(prompt: string): Observable<string> {
    const url = `${this.apiUrl}/querystream?prompt=${encodeURIComponent(prompt)}`;

    // Use 'responseType' as 'text' to handle plain text responses
    return new Observable((observer: any) => {
      const xhr = new XMLHttpRequest();
      xhr.open('GET', url, true);
      xhr.setRequestHeader('Accept', 'text/plain');
      let lastPosition = 0; // Keep track of the last processed position
      // Process each chunk as it's received
      xhr.onprogress = () => {
        const currentText = xhr.responseText;
        const newChunk = currentText.substring(lastPosition);
        lastPosition = currentText.length; // Update the last position
        observer.next(newChunk);
      };

      xhr.onload = () => {
        observer.complete();
      };

      xhr.onerror = () => {
        observer.error('An error occurred during streaming');
      };

      xhr.send();
    });
  }
}
