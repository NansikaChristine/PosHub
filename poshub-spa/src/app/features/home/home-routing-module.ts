import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home.component';
import { ItemComponent } from './item/item.component';
import { CategoryComponent } from './category/category.component';
import { OrderComponent } from './order/order.component';
import { ModifierComponent } from './modifier/modifier.component';

const routes: Routes = [
  { path: '', component: HomeComponent},
  { path: 'item', component: ItemComponent},
  { path: 'category', component: CategoryComponent},
  { path: 'order', component: OrderComponent},
  { path: 'modifier', component: ModifierComponent}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class HomeRoutingModule { }
