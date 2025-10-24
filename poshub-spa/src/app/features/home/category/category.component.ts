import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastService } from '../../../services/toast.service';
import { PosHubCatalogService } from '../../../services/PosHubCatalog.service';
import { ClientsDto } from '../../../dtos/clientsDto';

@Component({
  selector: 'app-category',
  standalone: false,
  templateUrl: './category.component.html',
  styleUrls: ['./category.component.scss']
})
export class CategoryComponent implements OnInit {
  categoryPosRef: string = '';
  currentClient!: ClientsDto;

  constructor(private router:Router, public toasterService: ToastService,public posHubService: PosHubCatalogService) { }

  ngOnInit() {
    const currentClientStr = localStorage.getItem('currentClient')
        if (currentClientStr) {
          this.currentClient = JSON.parse(currentClientStr) as ClientsDto;
        } else{
          this.toasterService.error('No client data found');
          this.router.navigate(['/']);
        }
  }

   public deleteCatalogCategoryByPosRefId(posRefId: string): void {
    const applicationId = this.currentClient.applicationId;
    if (!posRefId?.trim()) {
      this.toasterService.warning('Please enter a valid PosRefId', 'Validation');
      return;
    }

    const confirmed = confirm(`Are you sure you want to delete category: ${posRefId}?`);
    if (!confirmed) {
      return;
    }

    this.posHubService.deleteCatalogCategoryByPosRefId(applicationId, posRefId).subscribe({
      next: () => {
        this.toasterService.success(`Catalog category deleted: ${posRefId}`, 'Deleted');
      },
      error: (error) => {
        const status = error.status;
        if (status === 404) {
          this.toasterService.warning(error.error?.error || 'Category not found or already deleted.', 'Not Found');
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

}
