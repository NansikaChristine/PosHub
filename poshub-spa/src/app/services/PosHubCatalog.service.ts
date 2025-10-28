import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ClientsDto } from "../dtos/clientsDto";
import { CatalogProductsResponseDto } from "../dtos/catalogProductsResponseDto";
import { UpdateOrderEventRequestUIDto } from "../dtos/updateOrderEventRequestUIDto";
import { ProductDto } from "../dtos/productDto";
import { CatalogModifiersResponseDto } from "../dtos/catalogModifiersResponseDto";
import { ModifierDto } from "../dtos/modifierDto";

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

  constructor(private http: HttpClient) { }

  public SyncCatalogToPosHub(applicationId: string): Observable<boolean> {
    const url = environment.apiUrl + 'Catalog/syncCatalogToPosHub/' + applicationId;
    return this.http.post<boolean>(url, {});
  }

  public GetCatalogProducts(): Observable<ProductDto[]> {
    const url = environment.apiUrl + 'Catalog/getCatalogProducts';
    return this.http.get<ProductDto[]>(url, {});
  }

  public getClientsDetails() {
    const url = environment.apiUrl + 'PosHubAuthApi/apps';
    return this.http.get<any>(url, {});
  }

  public getCatalogProductsByPosRefId(applicationId: string, posRefId: string): Observable<any> {
    const url = environment.apiUrl + 'Catalog/getCatalogProductByPosRefId/' + applicationId + '/' + posRefId;
    return this.http.get<any>(url);
  }

  public deleteCatalogProductsByPosRefId(applicationId: string, posRefId: string): Observable<any> {
    const url = environment.apiUrl + 'Catalog/deleteCatalogProductByPosRefId/' + applicationId + '/' + posRefId;
    return this.http.delete<any>(url);
  }
  public deleteCatalogCategoryByPosRefId(applicationId: string, posRefId: string): Observable<any> {
    const url = environment.apiUrl + 'Catalog/deleteCatalogCategoryByPosRefId/' + applicationId + '/' + posRefId;
    return this.http.delete<any>(url);
  }
  public updateOrderEventByOrderId(dto: UpdateOrderEventRequestUIDto, applicationId: string): Observable<any> {
    const url = environment.apiUrl + 'OrderEvent/updateOrderEventByOrderId/' + applicationId;
    return this.http.put<any>(url, dto);
  }
  public updateProductByPosRefId(dto: ProductDto, applicationId: string): Observable<any> {
    const url = environment.apiUrl + 'Catalog/updateProductByPosRefId/' + applicationId;
    return this.http.patch<any>(url, dto);
  }
  public deleteAuthorize(applicationId: string): Observable<any> {
    const url = environment.apiUrl + 'PosHubAuthApi/deleteAuthorize/' + applicationId;
    return this.http.put<any>(url, {});
  }

  public GetCatalogModifiers(applicationId: string, limit: string): Observable<CatalogModifiersResponseDto> {
    const url = environment.apiUrl + 'Catalog/getCatalogModifiers/' + applicationId + '/' + limit;
    return this.http.get<CatalogModifiersResponseDto>(url, {});
  }
  public updateModifierByPosRefId(dto: ModifierDto, applicationId: string): Observable<any> {
    const url = environment.apiUrl + 'Catalog/updateModifierByPosRefId/' + applicationId;
    return this.http.patch<any>(url, dto);
  }
}