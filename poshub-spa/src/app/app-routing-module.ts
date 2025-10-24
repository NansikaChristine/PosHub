import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthRequestComponent } from './features/auth-request/auth-request.component';

const routes: Routes = [
  { path: '', component: AuthRequestComponent },
  { path: 'home', loadChildren: () => import('./features/home/home-module').then(m => m.HomeModule) },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
