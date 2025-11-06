import { ChangeDetectorRef, Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { ToastService } from '../../../services/toast.service';
import { PosHubCatalogService } from '../../../services/PosHubCatalog.service';
import { ClientsDto } from '../../../dtos/clientsDto';
import { Router } from '@angular/router';
import { ProductDto } from '../../../dtos/productDto';

@Component({
  selector: 'app-item',
  standalone: false,
  templateUrl: './item.component.html',
  styleUrls: ['./item.component.scss']
})
export class ItemComponent implements OnInit {
  showProductsTable = true;
  showProductsEdit = false;
  productPosRef: string = '';
  currentClient!: ClientsDto;
  selectedProduct!: ProductDto;
  products: ProductDto[] = [];



  constructor(
    private router: Router,
    public toasterService: ToastService,
    public posHubService: PosHubCatalogService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    const currentClientStr = localStorage.getItem('currentClient')
    if (currentClientStr) {
      this.currentClient = JSON.parse(currentClientStr) as ClientsDto;
    } else {
      this.toasterService.error('No client data found');
      this.router.navigate(['/']);
    }
  }

  public deleteCatalogProductsByPosRefId(product: ProductDto): void {
    if (!product.posReference?.trim()) {
      this.toasterService.warning('Please enter a valid PosRefId', 'Validation');
      return;
    }

    const confirmed = confirm(`Are you sure you want to delete product: ${product.posReference}?`);
    if (!confirmed) {
      return;
    }

    this.posHubService.deleteCatalogProductsByPosRefId(this.currentClient.applicationId, product.posReference).subscribe({
      next: () => {
        this.toasterService.success(`Catalog product deleted: ${product.posReference}`, 'Deleted');
      },
      error: (error) => {
        const status = error.status;
        if (status === 404) {
          this.toasterService.warning(error.error?.error || 'Product not found or already deleted.', 'Not Found');
        } else if (status === 401) {
          this.toasterService.error('Unauthorized access. Please login.', 'Unauthorized');
        } else if (status === 403) {
          this.toasterService.error('You are not allowed to perform this action.', 'Forbidden');
        } else {
          this.toasterService.error(error.error?.error || 'Something went wrong while deleting.', 'Error');
        }
      }
    });
  }

  public getCatalogProducts(): void {
    this.posHubService.GetCatalogProducts().subscribe({
      next: (res) => {
        this.products = res;
        this.showProductsTable = true;
        this.showProductsEdit = false;
        this.cdr.detectChanges();
        this.toasterService.success('Get Catalog products successfully:');
      },
      error: (err) => {
        this.toasterService.error('Error getting Catalog products:', err);
      }
    });
  }

  getCatalogProductsByPosRefId(applicationId: string, posRefId: string) {
    if (!posRefId) {
      this.toasterService.warning('Please enter a valid PosRefId', 'Validation');
      return;
    }

    this.posHubService.getCatalogProductsByPosRefId(applicationId, posRefId)
      .subscribe(response => {
        this.selectedProduct = response;
        console.log(response);
      }, error => {
        this.toasterService.error('Error fetching product', error);
      });
  }

  selectProduct(product: ProductDto) {
    this.selectedProduct = { ...product };
    this.showProductsEdit = true;
    this.showProductsTable = false;
  }

  clearProduct() {
    this.selectedProduct = undefined!;
    this.showProductsEdit = false;
    this.showProductsTable = true;
  }

  
  updateProduct() {
    this.selectedProduct.price = this.selectedProduct.price * 100;
    this.posHubService.updateProductByPosRefId(this.selectedProduct, this.currentClient.applicationId).subscribe({
      next: (res) => {
        this.toasterService.success('Product update successfully:');
        this.getCatalogProducts();
        this.showProductsTable = true;
        this.showProductsEdit = false;
      },
      error: (err) => {
        this.toasterService.error('Error update Catalog products:', err);
      }
    });
  }

  public backPage():void{
    this.router.navigate(['/home']);
  }
}
