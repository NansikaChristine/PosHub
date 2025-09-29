import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ClientsDto } from "../dtos/clientsDto";
import { CatalogProductsResponseDto } from "../dtos/catalogProductsResponseDto";

/**
 * @description
 * @class
 */
@Injectable({
  providedIn: 'root'
})
export class PosHubCatalogService {
  public clientDetails: ClientsDto[] = [];
  client?: ClientsDto;
  
  constructor(private http: HttpClient) {}

  public SyncCatalogToPosHub(client: ClientsDto): Observable<boolean>  {
    const url = environment.apiUrl + 'Catalog/syncCatalogToPosHub/' + client.applicationId;
    return this.http.post<boolean>(url, {});
  }

  public GetCatalogProducts(client: ClientsDto, limit: string): Observable<CatalogProductsResponseDto> {
    const url = environment.apiUrl + 'Catalog/catalogProducts/' + client.applicationId + '/' + limit;
    return this.http.get<CatalogProductsResponseDto>(url, {});
  }

  public getClientsDetails(){
    const url = environment.apiUrl + 'PosHubAuthApi/apps';
    return this.http.get<any>(url, {}); 
  }
}