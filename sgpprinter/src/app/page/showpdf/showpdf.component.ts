import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-showpdf',
  templateUrl: './showpdf.component.html',
  styleUrls: ['./showpdf.component.css']
})
export class ShowpdfComponent implements OnInit {

  src;
  constructor() { }

  ngOnInit() {
    this.src  = "http://localhost:49334/api/report/PhieuGui?" + encodeURIComponent("dateFrom=01/01/2019&dateTo=01/10/2019");
  }

}
