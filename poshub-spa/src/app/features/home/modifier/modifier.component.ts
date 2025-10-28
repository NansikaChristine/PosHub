import { ChangeDetectorRef, Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { ToastService } from '../../../services/toast.service';
import { PosHubCatalogService } from '../../../services/PosHubCatalog.service';
import { ClientsDto } from '../../../dtos/clientsDto';
import { Router } from '@angular/router';
import { ModifierDto } from '../../../dtos/modifierDto';

@Component({
  selector: 'app-modifier',
  standalone: false,
  templateUrl: './modifier.component.html',
  styleUrls: ['./modifier.component.css']
})

export class ModifierComponent implements OnInit {
  showModifiersTable = true;
  showModifiersEdit = false;
  modifierPosRef: string = '';
  currentClient!: ClientsDto;
  selectedModifier!: ModifierDto;
  modifiers: ModifierDto[] = [];



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

  // public deleteCatalogProductsByPosRefId(product: ProductDto): void {
  //   if (!product.posReference?.trim()) {
  //     this.toasterService.warning('Please enter a valid PosRefId', 'Validation');
  //     return;
  //   }

  //   const confirmed = confirm(`Are you sure you want to delete product: ${product.posReference}?`);
  //   if (!confirmed) {
  //     return;
  //   }

  //   this.posHubService.deleteCatalogProductsByPosRefId(this.currentClient.applicationId, product.posReference).subscribe({
  //     next: () => {
  //       this.toasterService.success(`Catalog product deleted: ${product.posReference}`, 'Deleted');
  //     },
  //     error: (error) => {
  //       const status = error.status;
  //       if (status === 404) {
  //         this.toasterService.warning(error.error?.error || 'Product not found or already deleted.', 'Not Found');
  //       } else if (status === 401) {
  //         this.toasterService.error('Unauthorized access. Please login.', 'Unauthorized');
  //       } else if (status === 403) {
  //         this.toasterService.error('You are not allowed to perform this action.', 'Forbidden');
  //       } else {
  //         this.toasterService.error(error.error?.error || 'Something went wrong while deleting.', 'Error');
  //       }
  //     }
  //   });
  // }

  public getCatalogModifiers(limit: string): void {
    this.posHubService.GetCatalogModifiers(this.currentClient.applicationId, limit).subscribe({
      next: (res) => {
        this.modifiers = res.data;
        this.showModifiersTable = true;
        this.showModifiersEdit = false;
        this.cdr.detectChanges();
        this.toasterService.success('Get Catalog modifiers successfully:');
      },
      error: (err) => {
        this.toasterService.error('Error getting Catalog modifiers:', err);
      }
    });
  }

  // getCatalogProductsByPosRefId(applicationId: string, posRefId: string) {
  //   if (!posRefId) {
  //     this.toasterService.warning('Please enter a valid PosRefId', 'Validation');
  //     return;
  //   }

  //   this.posHubService.getCatalogProductsByPosRefId(applicationId, posRefId)
  //     .subscribe(response => {
  //       this.selectedProduct = response;
  //       console.log(response);
  //     }, error => {
  //       this.toasterService.error('Error fetching product', error);
  //     });
  // }

  selectModifier(modifier: ModifierDto) {
    this.selectedModifier = { ...modifier };
    this.showModifiersEdit = true;
    this.showModifiersTable = false;
  }

  clearModifier() {
    this.selectedModifier = undefined!;
    this.showModifiersEdit = false;
    this.showModifiersTable = true;
  }

  
  updateModifier() {
    console.log(this.selectedModifier);
    this.posHubService.updateModifierByPosRefId(this.selectedModifier, this.currentClient.applicationId).subscribe({
      next: (res) => {
        this.toasterService.success('Modifier update successfully:');
        this.getCatalogModifiers('1');
        this.showModifiersTable = true;
        this.showModifiersEdit = false;
      },
      error: (err) => {
        this.toasterService.error('Error update Catalog modifiers:', err);
      }
    });
  }

  public backPage():void{
    this.router.navigate(['/home']);
  }
}
