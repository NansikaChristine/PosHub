import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastService } from '../../../services/toast.service';
import { PosHubCatalogService } from '../../../services/PosHubCatalog.service';
import { ClientsDto } from '../../../dtos/clientsDto';
import { UpdateOrderEventRequestUIDto } from '../../../dtos/updateOrderEventRequestUIDto';

@Component({
  selector: 'app-order',
  standalone: false,
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.scss']
})
export class OrderComponent implements OnInit {

  currentClient!: ClientsDto
  orderInputs: { [appId: string]: { orderId: string, status: string, reason: string } } = {};  
  statuses: string[] = ['ACCEPTED', 'COMPLETED', 'REJECTED', 'CANCELLED', 'READY'];
  
  constructor(private router:Router, public toasterService: ToastService,
     private cdr: ChangeDetectorRef,public posHubService: PosHubCatalogService) { }

  ngOnInit() {
    const currentClientStr = localStorage.getItem('currentClient')
        if (currentClientStr) {
          this.currentClient = JSON.parse(currentClientStr) as ClientsDto;
          this.orderInputs[this.currentClient.applicationId] = {
            orderId: '',
            status: 'ACCEPTED',
            reason: ''
          };
        } else{
          this.toasterService.error('No client data found');
          this.router.navigate(['/']);
        }
  }

  onStatusChange(appId: string): void {
    const inputs = this.orderInputs[appId];
    if (inputs.status !== 'CANCELLED') {
      inputs.reason = '';
    }
  }

  updateOrderEvent(applicationId: string): void {
      const inputs = this.orderInputs[applicationId];
  
      const dto = new UpdateOrderEventRequestUIDto();
      dto.orderId = inputs.orderId;
      dto.status = inputs.status;
      dto.cancellationReason = inputs.reason;
      console.log(dto);
  
      this.posHubService.updateOrderEventByOrderId(dto, applicationId).subscribe({
        next: (response) => {
          this.toasterService.success('Order event updated successfully!');
  
          inputs.orderId = '';
          inputs.status = 'ACCEPTED';
          inputs.reason = '';
  
          this.onStatusChange(applicationId);
          this.cdr.detectChanges();
          console.log(response);
        },
        error: (err) => {
          if (err.status === 404) {
            this.toasterService.error(err.error?.message || 'Order not found.');
          } else if (err.status === 400) {
            this.toasterService.error(err.error?.message || 'Bad request.');
          } else if (err.status === 500) {
            this.toasterService.error('Server error: ' + (err.error?.message || 'Unknown error occurred.'));
          } else {
            this.toasterService.error('Unexpected error: ' + JSON.stringify(err.error));
          }
          console.error(err);
        }
      });
    }
}
