import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {HomeComponent} from './page/home/home.component';
import {ShowpdfComponent} from './page/showpdf/showpdf.component';
import {LoginComponent} from './page/login/login.component';
import {AuthGuardGuard} from './auth-guard.guard';
import {FptOrderComponent} from './page/fpt-order/fpt-order.component';
import { SgpmailerComponent } from './page/tracking/sgpmailer/sgpmailer.component';
import { SgporderComponent } from './page/sgporder/sgporder.component';


const routes: Routes = [
  {path: '', component: HomeComponent, canActivate: [AuthGuardGuard]},
  {path: 'fptorder', component: FptOrderComponent, canActivate: [AuthGuardGuard]},
  {path: 'print', component: ShowpdfComponent, canActivate: [AuthGuardGuard]},
  {path: 'login', component: LoginComponent},
  {path: 'sgpmailer', component: SgpmailerComponent},
  {path: 'sgporder', component: SgporderComponent, canActivate: [AuthGuardGuard]}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
