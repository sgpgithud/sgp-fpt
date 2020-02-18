import { Component, OnInit } from '@angular/core';
import { decodeBase64 } from '../../utils/settings';
import { Router } from '@angular/router';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.css']
})
export class TopbarComponent implements OnInit {

  constructor(private router: Router) { }
  user;
  ngOnInit() {
    const data = localStorage.getItem('sgpinfo');

    if (data) {

      const infoDecode = decodeBase64(data);

      const infos = JSON.parse(infoDecode);
      console.log(infos);
      this.user = infos.userName;
      console.log(this.user);
    }
  }


  logout() {
    localStorage.removeItem('sgpinfo');
    this.router.navigate(['/login']);
  }

}
