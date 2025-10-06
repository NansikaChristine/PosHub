import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PosHubCatalogService } from '../../services/PosHubCatalog.service';
import { TokenRequestDto } from '../../dtos/tokenRequestDto';
import { TokenResponseDto } from '../../dtos/tokenResponseDto';
import { AccountLocationDto } from '../../dtos/accountLocationDto';
import { Router } from '@angular/router';
import { CatalogImportEntityDto } from '../../dtos/catalogImportEntityDto';
import { ClientsDto } from '../../dtos/clientsDto';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UpdateOrderEventRequestDto } from '../../dtos/updateOrderEventRequestDto';

@Component({
  selector: 'app-auth-request',
  standalone: false,
  templateUrl: './auth-request.component.html',
  styleUrls: ['./auth-request.component.scss']
})
export class AuthRequestComponent implements OnInit {
  scopes: string[] = []; // you can populate this with actual scopes
  response?: TokenResponseDto;
  storeDetails: any;
  catalog: CatalogImportEntityDto | null = null;
  searchTerm: string = '';
  filteredClientDetails: ClientsDto[] = [];
  authorized: boolean = false;
  selectedProduct: any = null;
  orderSearchTerm: string = '';
  orderInputs: { [appId: string]: { orderId: string, status: string, reason: string } } = {};
  statuses: string[] = ['ACCEPTED', 'COMPLETED', 'REJECTED', 'CANCELLED'];


  constructor(
    public service: PosHubCatalogService,
    private cdr: ChangeDetectorRef,
    public http: HttpClient
  ) { }

  ngOnInit(): void {
    this.getClientDetails();
  }
  onStatusChange(appId: string): void {
    const inputs = this.orderInputs[appId];
    if (inputs.status !== 'CANCELLED') {
      inputs.reason = '';
    }
  }

  updateOrderEvent(appId: string): void {
    const inputs = this.orderInputs[appId];

    const dto = new UpdateOrderEventRequestDto();
    dto.orderId = inputs.orderId;
    dto.status = inputs.status;
    dto.cancellationReason = inputs.reason;

    this.service.updateOrderEventByOrderId(dto).subscribe({
      next: (response) => {
        alert('Order event updated successfully!');
        
        inputs.orderId = '';
        inputs.status = 'ACCEPTED';
        inputs.reason = '';

        this.onStatusChange(appId);
        this.cdr.detectChanges();
        console.log(response);
      },
      error: (err) => {
        if (err.status === 404) {
          alert(err.error?.message || 'Order not found.');
        } else if (err.status === 400) {
          alert(err.error?.message || 'Bad request.');
        } else if (err.status === 500) {
          alert('Server error: ' + (err.error?.message || 'Unknown error occurred.'));
        } else {
          alert('Unexpected error: ' + JSON.stringify(err.error));
        }
        console.error(err);
      }
    });
  }

  public getClientDetails(): void {
    this.service.getClientsDetails().subscribe({
      next: (data: ClientsDto[]) => {
        this.service.clientDetails = data || [];
        this.filteredClientDetails = [...this.service.clientDetails];

        this.filteredClientDetails.forEach(client => {
          this.orderInputs[client.applicationId] = {
            orderId: '',
            status: 'ACCEPTED',
            reason: ''
          };
        });
        this.cdr.detectChanges();
        console.log(this.filteredClientDetails);
      },
      error: (err) => console.error('Error fetching clients:', err)
    });
  }

  public applyFilter(): void {
    const term = this.searchTerm.toLowerCase().trim();
    if (term === '') {
      console.log('Clients:', this.filteredClientDetails);
      this.filteredClientDetails = [...this.service.clientDetails]; // show all
    } else {
      console.log('Clients:', this.filteredClientDetails);
      this.filteredClientDetails = this.service.clientDetails.filter(app =>
        app.clientName.toLowerCase().includes(term)
      );
    }
  }

  public authorize(client: ClientsDto): void {
    this.service.client = client;
    console.log(this.service.client);


    const scopes = encodeURIComponent("provisioning connections.write catalogs.read catalogs.write orders.read orders.write locations.read locations.write connections.read");

    const url = `https://api-sit-dr.stage.tryposhub.com/oauth2/authorize?` +
      `client_id=${client.clientId}&` +
      `redirect_uri=${client.redirectUrl}&` +
      `response_type=code&` +
      `scope=${scopes}&` +
      `state=1234`;

    window.location.href = url;
  }

  public syncCatalogToPosHub(applicationId: string): void {
    console.log(applicationId);
    this.service.SyncCatalogToPosHub(applicationId).subscribe({
      next: (res) => {
        console.log('Catalog synced successfully:', res);
      },
      error: (err) => {
        console.error('Error syncing catalog:', err);
      }
    });
  }

  public getCatalogProducts(applicationId: string, limit: string): void {
    console.log(applicationId);
    this.service.GetCatalogProducts(applicationId, limit).subscribe({
      next: (res) => {
        console.log('Get Catalog products successfully:', res);
      },
      error: (err) => {
        console.error('Error getting Catalog products:', err);
      }
    });
  }

  getCatalogProductsByPosRefId(applicationId: string, posRefId: string) {
    if (!posRefId) {
      alert('Please enter a PosRefId');
      return;
    }

    this.service.getCatalogProductsByPosRefId(applicationId, posRefId)
      .subscribe(response => {
        this.selectedProduct = response;
        console.log(response);
      }, error => {
        console.error('Error fetching product', error);
        this.selectedProduct = null;
      });
  }

}