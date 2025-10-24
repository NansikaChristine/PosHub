import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class ToastService {

constructor(private toastr: ToastrService) {}

  success(message: string, title: string = 'Success') {
    this.toastr.success(message, title, {
      progressBar: true,
      timeOut: 4000,
      //closeButton: true
    });
  }

  error(message: string, title: string = 'Error') {
    this.toastr.error(message, title, {
      progressBar: true,
      timeOut: 7000,
     // closeButton: true,
      //tapToDismiss: false
    });
  }

  info(message: string, title: string = 'Info') {
    this.toastr.info(message, title, {
      //progressBar: true
    });
  }

  warning(message: string, title: string = 'Warning') {
    this.toastr.warning(message, title, {
      //closeButton: true
    });
  }

  custom(message: string, title: string, options: any = {}) {
    this.toastr.show(message, title, options);
  }
}