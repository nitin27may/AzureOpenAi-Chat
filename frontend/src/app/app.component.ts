import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChatComponent } from "./chat/chat.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ChatComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'simplechat';
}
