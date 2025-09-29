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

  constructor(
    public service: PosHubCatalogService,
    private cdr: ChangeDetectorRef,
    public http: HttpClient
  ) { }

  ngOnInit(): void {
    this.getClientDetails();
  }

  public getClientDetails(): void {
    this.service.getClientsDetails().subscribe({
      next: (data: ClientsDto[]) => {
        this.service.clientDetails = data || [];
        this.filteredClientDetails = [...this.service.clientDetails];
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

  public syncCatalogToPosHub(client: ClientsDto): void {
    console.log(client);
    this.service.SyncCatalogToPosHub(client).subscribe({
      next: (res) => {
        console.log('Catalog synced successfully:', res);
      },
      error: (err) => {
        console.error('Error syncing catalog:', err);
      }
    });
  }

  public getCatalogProducts(client: ClientsDto, limit:string): void {
    console.log(client);
    this.service.GetCatalogProducts(client, limit).subscribe({
      next: (res) => {
        console.log('Get Catalog products successfully:', res);
      },
      error: (err) => {
        console.error('Error getting Catalog products:', err);
      }
    });
  }
}


