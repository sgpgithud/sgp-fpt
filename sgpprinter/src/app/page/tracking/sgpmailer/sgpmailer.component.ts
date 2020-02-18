import { Component, OnInit } from '@angular/core';
import { errorHandle, showLoader } from '../../../utils/settings';
import { FormBuilder, FormGroup } from '@angular/forms';
declare var $: any;
import { formatDate } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-sgpmailer',
  templateUrl: './sgpmailer.component.html',
  styleUrls: ['./sgpmailer.component.css']
})
export class SgpmailerComponent implements OnInit {


  employees: any;

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {

    this.httpClient.get('http://api.sgp.vn/api/Tracking/GetAllEmployee').subscribe((p: any) => {
      this.employees = p.data;
    });
  }

}
