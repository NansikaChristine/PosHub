import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';
import { HomeRoutingModule } from './home-routing-module';
import { ItemComponent } from './item/item.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { CategoryComponent } from './category/category.component';
import { OrderComponent } from './order/order.component';


@NgModule({
  declarations: [
    HomeComponent,
    ItemComponent,
    CategoryComponent,
    OrderComponent
  ],
  imports: [
    CommonModule,
    HomeRoutingModule,
    FormsModule,
    HttpClientModule
  ]
})
export class HomeModule { }
