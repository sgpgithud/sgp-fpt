import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../service/auth.service';
import { Router } from '@angular/router';
import { UserToken } from '../../models/data.models';
import { FormBuilder } from '@angular/forms';
import { encodeBase64 } from '../../utils/settings';
import { errorHandle, showLoader } from '../../utils/settings';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(private auth: AuthService, public router: Router, private formBuilder: FormBuilder) {
  }

  loginForm;

  login(data) {
    showLoader(true);
    this.auth.login(data.username, data.password).subscribe((data: UserToken) => {
      showLoader(false);

      var dataEncode = encodeBase64(JSON.stringify(data));

      localStorage.setItem('sgpinfo', dataEncode);

      this.router.navigate(['/']);

    }, (error: any) => {
      showLoader(false);
      alert('Đăng nhập không thành công');
    });
  }

  ngOnInit() {
    let body = document.getElementsByTagName('body')[0];
    body.classList.add("bg-gradient-primary");

    this.loginForm = this.formBuilder.group({
      'username': '',
      'password': ''
    });



  }

}
