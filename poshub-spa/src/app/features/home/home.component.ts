import { Component, OnInit } from '@angular/core';
import { ClientsDto } from '../../dtos/clientsDto';
import { Router } from '@angular/router';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  currentClient!: ClientsDto

  constructor(private router: Router,
    public toasterService: ToastService) { }

  ngOnInit() {
    const currentClientStr = localStorage.getItem('currentClient')
    if (currentClientStr) {
      this.currentClient = JSON.parse(currentClientStr) as ClientsDto;
    } else {
      this.toasterService.error('No client data found');
      this.router.navigate(['/']);
    }
  }


  public viewItem(): void {
    this.router.navigate(['home/item']);
  }

  public viewCategory(): void {
    this.router.navigate(['home/category']);
  }

  public viewOrder(): void {
    this.router.navigate(['home/order']);
  }

}
