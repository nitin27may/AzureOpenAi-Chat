import { Component, input, output, signal } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";

@Component({
  selector: 'app-form-field',
  imports: [FormsModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  templateUrl: './form-field.component.html',
  styleUrl: './form-field.component.css'
})
export class FormFieldComponent {
  readonly step = input<number>();
  readonly type = input<string>();
  readonly control = input.required<FormControl>();
  readonly selectOptions = input<string[]>();

  readonly nextStepEvent = output<{ message: string; files?: File[] }>();
  readonly selectedFiles = signal<File[]>([]);
  handleSend(): void {
    const control = this.control();
    const message = control.value;
    const files = this.selectedFiles();
    
    if (message || files.length > 0) {
      this.nextStepEvent.emit({ message, files });
      control.reset();
      this.selectedFiles.set([]);
    }
  }

  handleFileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFiles.set(Array.from(input.files));
    }
  }
}
