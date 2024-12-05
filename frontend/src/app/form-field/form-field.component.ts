import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";

@Component({
  selector: 'app-form-field',
  imports: [CommonModule, FormsModule,ReactiveFormsModule, MatFormFieldModule,MatInputModule,MatButtonModule, MatIconModule],
  templateUrl: './form-field.component.html',
  styleUrl: './form-field.component.css'
})
export class FormFieldComponent {
  @Input() step!: number;
  @Input() type! : string;
  @Input() control! : FormControl;
  @Input() selectOptions! : string[] | undefined;

  @Output() nextStepEvent = new EventEmitter();
  selectedFiles: File[] = [];
  handleSend () {
    const message = this.control.value;
    if (message || this.selectedFiles.length > 0) {
      this.nextStepEvent.emit({ message, files: this.selectedFiles });
      this.control.reset();
      this.selectedFiles = [];
     // (fileInput as HTMLInputElement).value = '';
    }

    if (this.control.valid) {
      this.nextStepEvent.emit(this.control.value);
    } else {
      console.log(this.control.errors);
    }
  }
  handleFileInput(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFiles = Array.from(input.files);
    }
  }
}
