import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './service/auth.service';
import { decodeBase64 } from './utils/settings';
import { UserToken } from './models/data.models';

@Injectable({
  providedIn: 'root'
})
export class AuthGuardGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {
  }

  canActivate(next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
    const url: string = state.url;

    return this.checkLogin(url);
  }

  checkLogin(url: string): boolean {

    const data = localStorage.getItem('sgpinfo');

    if (data) {

      const infoDecode = decodeBase64(data);

      const infos = JSON.parse(infoDecode);

      if (infos.access_token) {
        return true;
      }
    }
    // Navigate to the login page with extras
    this.router.navigate(['/login']);
    return false;
  }
}
