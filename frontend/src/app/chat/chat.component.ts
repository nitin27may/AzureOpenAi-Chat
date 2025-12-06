import {
  Component,
  ElementRef,
  viewChild,
  signal,
  inject,
  afterNextRender,
} from '@angular/core';
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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  trigger,
  style,
  transition,
  animate,
} from '@angular/animations';

interface Message {
  content: string;
  user: boolean;
}

@Component({
  selector: 'app-chat',
  imports: [
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
  styleUrl: './chat.component.css',
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('500ms ease-in', style({ opacity: 1 })),
      ]),
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(-100%)', opacity: 0 }),
        animate(
          '500ms ease-out',
          style({ transform: 'translateX(0)', opacity: 1 })
        ),
      ]),
    ]),
  ],
})
export class ChatComponent {
  private readonly fb = inject(FormBuilder);
  private readonly chatService = inject(ChatService);

  readonly scrollFrame = viewChild<ElementRef>('scrollMe');
  
  readonly isLoading = signal(false);
  readonly messages = signal<Message[]>([]);
  readonly step = signal(0);

  readonly form = this.fb.group({
    message: new FormControl('', Validators.required),
  });

  constructor() {
    afterNextRender(() => {
      // Scroll container is available after render
    });
  }

  addMessage(content: string, user: boolean): void {
    this.messages.update(messages => {
      if (!user && messages.length > 0 && !messages[messages.length - 1].user) {
        // Append content to the last bot message
        const updatedMessages = [...messages];
        updatedMessages[updatedMessages.length - 1] = {
          ...updatedMessages[updatedMessages.length - 1],
          content: updatedMessages[updatedMessages.length - 1].content + content
        };
        return updatedMessages;
      }
      // Create a new message
      return [...messages, { content, user }];
    });
    this.scrollToBottom();
  }

  handleNewInfo(event: { message: string; files?: File[] }): void {
    this.isLoading.set(true);
    const { message, files } = event;
    let finalMessage = '';
    
    if (files && files.length > 0) {
      finalMessage = files.map(file => file.name).join('<br>') + '<hr>';
    }
    
    if (message) {
      finalMessage += message;
      this.addMessage(finalMessage, true);
    }

    this.chatService.sendMessage(this.form.value.message ?? '', files).subscribe({
      next: (chunk) => {
        this.addMessage(chunk, false);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Chat error:', err);
        this.isLoading.set(false);
      },
      complete: () => {
        console.log('Streaming complete');
      },
    });
  }

  scrollToBottom(): void {
    const scrollContainer = this.scrollFrame()?.nativeElement;
    if (scrollContainer) {
      scrollContainer.scroll({
        top: scrollContainer.scrollHeight,
        left: 0,
        behavior: 'smooth',
      });
    }
  }
}
