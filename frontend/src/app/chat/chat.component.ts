import {
  Component,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ChatService } from '../chat.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormFieldComponent } from '../form-field/form-field.component';
import { MarkdownComponent } from 'ngx-markdown';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner'
import {
  trigger,
  style,
  transition,
  animate,
  keyframes,
} from '@angular/animations';
interface Message {
  content: string;
  user: boolean;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    MarkdownComponent,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    FormFieldComponent,
    MatProgressSpinnerModule
  ],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }), // Initial state
        animate('500ms ease-in', style({ opacity: 1 })), // Final state
      ]),
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(-100%)', opacity: 0 }), // Initial state
        animate(
          '500ms ease-out',
          style({ transform: 'translateX(0)', opacity: 1 }) // Final state
        ),
      ]),
    ]),
  ],
})
export class ChatComponent {
  isLoading: boolean = false;
  @ViewChild('scrollMe', { static: false }) scrollFrame: ElementRef | undefined;

  private scrollContainer: any;

  step: number = 0;
  messages: Message[] = [];
  form: any;

  constructor(private fb: FormBuilder, private chatService: ChatService) {
    this.form = this.fb.group({
      message: new FormControl('', Validators.required),
    });
  }
  ngAfterViewInit(): void {
    this.scrollContainer = this.scrollFrame!.nativeElement;
    //this.addMessage(this.questions[this.step], false);
  }

  ngOnInit(): void {}

  addMessage(content: string, user: boolean) {
    if (!user && this.messages.length > 0 && !this.messages[this.messages.length - 1].user) {
      // Append content to the last bot message
      this.messages[this.messages.length - 1].content += content;
    } else {
      // Create a new message
      this.messages.push({ content, user });
    }
    this.scrollToBottom();
  }

  handleNewInfo(event: { message: string; files?: File[] }) {
    this.isLoading = true;
    const { message, files } = event;
    var finalmessage = '';
    if (files && files.length > 0) {
      files.forEach((file) => {
        finalmessage += file.name + '<br>';
      });
      finalmessage += '<hr>';
    }
    if (message) {
      finalmessage += message;
      this.addMessage(finalmessage, true);
    }

    this.chatService.sendMessage(this.form.value.message, files).subscribe({
      next: (chunk: any) => {
        console.log('response chunk', chunk);
        this.addMessage(chunk, false); // Append to the last bot message
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error(err);
        this.isLoading = false;
      },
      complete: () => {
        console.log('Streaming complete');
      },
    });
    // this.chatService.getStreamingResponse(this.form.value.message).subscribe({
    //   next: (chunk: any) => {
    //     console.log('response chunk', chunk);
    //     this.addMessage(chunk, false); // Append to the last bot message
    //     this.isLoading = false;
    //   },
    //   error: (err: any) => {
    //     console.error(err);
    //     this.isLoading = false;
    //   },
    //   complete: () => {
    //     console.log('Streaming complete');
    //   },
    // });
  }

  scrollToBottom(): void {
    if (this.scrollContainer)
      this.scrollContainer.scroll({
        top: this.scrollContainer.scrollHeight,
        left: 0,
        behavior: 'smooth',
      });
  }
}
