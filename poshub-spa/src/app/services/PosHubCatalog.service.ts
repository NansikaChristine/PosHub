import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ClientsDto } from "../dtos/clientsDto";
import { CatalogProductsResponseDto } from "../dtos/catalogProductsResponseDto";
import { UpdateOrderEventRequestDto } from "../dtos/updateOrderEventRequestDto";

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

  public SyncCatalogToPosHub(applicationId: string): Observable<boolean>  {
    const url = environment.apiUrl + 'Catalog/syncCatalogToPosHub/' + applicationId;
    return this.http.post<boolean>(url, {});
  }

  public GetCatalogProducts(applicationId: string, limit: string): Observable<CatalogProductsResponseDto> {
    const url = environment.apiUrl + 'Catalog/getCatalogProducts/' + applicationId + '/' + limit;
    return this.http.get<CatalogProductsResponseDto>(url, {});
  }

  public getClientsDetails(){
    const url = environment.apiUrl + 'PosHubAuthApi/apps';
    return this.http.get<any>(url, {}); 
  }

  public getCatalogProductsByPosRefId(applicationId: string, posRefId: string): Observable<any> {
    const url = environment.apiUrl + 'Catalog/getCatalogProductByPosRefId/' + applicationId + '/' + posRefId;
    return this.http.get<any>(url);
  }

  public updateOrderEventByOrderId(dto:UpdateOrderEventRequestDto): Observable<any> {
    const url = environment.apiUrl + 'OrderEvent/updateOrderEventByOrderId';
    return this.http.put<any>(url,dto);
  }
}