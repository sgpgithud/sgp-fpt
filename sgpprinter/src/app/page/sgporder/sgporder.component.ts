import { Component, OnInit } from '@angular/core';
import { DataService } from '../../service/data.service';
import { MailerInfo, OrderStatus } from '../../models/data.models';
import { errorHandle, showLoader, showModel, hideModel } from '../../utils/settings';
import { FormBuilder, FormGroup } from '@angular/forms';
import { APIURL } from '../../utils/settings';
declare var $: any;
import { formatDate } from '@angular/common';


@Component({
  selector: 'app-sgporder',
  templateUrl: './sgporder.component.html',
  styleUrls: ['./sgporder.component.css']
})
export class SgporderComponent implements OnInit {


  rootUrl = APIURL;
  searchForm;
  interval;
  orders: any[];
  showSynButton = 0;
  OrderStatus: OrderStatus[];
  DeliveryStatus = [{ "code": 0, "name": "Tất cả" }, { "code": 6, "name": "Đang phát" }, { "code": 13, "name": "Chưa phát được" }];
  showconfig = false;
  customers = ['PSD', 'FPT'];

  provinces: any[];
  districts: any[];

  orderSevices = [{ "code": "SN", "name": "Nhanh"}, { "code": "ST", "name": "Thường"}];
  orderTypes = [{ "code": "HH", "name": "Hàng hóa"}, { "code": "T", "name": "Thư"}];

  constructor(private dataService: DataService, private formBuilder: FormBuilder) { }

  orderUpdate = {
    id : 0,
    mailerid: "",
    provincePMS : "",
    districtPMS: "",
    orderType: "",
    orderService: "",
    price: 0,
    priceService: 0,
    quantity: 1,
    weight: 0,
    address: "",
    notes: ""
  };

  ngOnInit() {
    var today = new Date();
    let dd = today.getDate() + '';
    let mm = (today.getMonth() + 1) + '';

    let yyyy = today.getFullYear();

    if (today.getDate() < 10) {
      dd = '0' + dd;
    }
    if ((today.getMonth() + 1) < 10) {
      mm = '0' + mm;
    }
    var dateStr = dd + '/' + mm + '/' + yyyy;

    this.searchForm = this.formBuilder.group({
      dateFrom: dateStr,
      dateTo: dateStr,
      status: 0,
      cusid: 'PSD',
      mailerid: '',
      deliveryStatus: 0
    });

    this.OrderStatus = [{ "code": 0, "name": "Đơn mới" },
    { "code": 3, "name": "Đã nhận hàng" }];

    this.interval = setInterval(() => {
      this.findMailer();
    },1000*3*60)

    this.findMailer();

    this.dataService.getProvince().subscribe((p: any) => {
      this.provinces = p.data;
      console.log(p.data);
    });

  }

  findDistrict() {
    this.dataService.getDistrict(this.orderUpdate.provincePMS).subscribe((p: any) => {
      this.districts = p.data;
    });
  }

  findMailer() {

    showLoader(true);
    this.dataService.getSGPOrder(this.searchForm.value).subscribe((data: any) => {
      showLoader(false);
      this.orders = data.data;
    },
      error => {
        showLoader(false);
        errorHandle(error);
      });
  }

  findStatus(code: number) {
    for (const item of this.OrderStatus) {
      if (item.code === code) {
        return item.name;
      }
    }

    return 'None';
  }

  printReport() {
    window.open(this.dataService.printReports(this.searchForm.value), '_blank');

    /*
    showLoader(true);
    this.dataService.printReports(this.searchForm.value).subscribe((data: any) => {
      showLoader(false);
      this.dataService.downloadFile(data, "application/pdf");
    },
      error => {
        showLoader(false);
        errorHandle(error);
      });*/
  }
  printOneReport(mailerid: number) {

    window.open(APIURL + 'Report/InMotPhieuPSD?id=' + mailerid, '_blank');

    /*
     showLoader(true);
    this.dataService.printOneReports(this.searchForm.value, mailerid).subscribe((data: any) => {
      showLoader(false);
      this.dataService.downloadFile(data, "application/pdf");
    },
      error => {
        showLoader(false);
        errorHandle(error);
      });*/
  }
  printExcels() {

    /*
    showLoader(true);
    this.dataService.printExcels(this.searchForm.value).subscribe((data: any) => {
      showLoader(false);
      console.log(data);
      this.dataService.downloadFile(data, "application/vnd.ms-excel");
    },
      error => {
        showLoader(false);
        errorHandle(error);
      });
      */
    var data = this.searchForm.value;
    var url = APIURL + 'api/sgphub/GetOrderExcel?dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status + '&mailerid=' +  data.mailerid  + '&cusid=' + data.cusid;
    window.open(url, '_blank');

  }

  showSearch() {
    this.showconfig = !this.showconfig;
  }

  changeStatus() {
    //console.log(this.searchForm.value.status);
    if (this.searchForm.value.status == 3) {
      this.showSynButton = 1;
    } else { this.showSynButton = 0; }

    this.findMailer();
  }


  showUpdateOrder(item) {
    this.orderUpdate.id = item.Id;
    this.orderUpdate.mailerid = item.OrderCode;
    this.orderUpdate.provincePMS = item.ReceiverProvincePMS;
    this.orderUpdate.districtPMS = item.ReceiverDistrictPMS;
    this.orderUpdate.orderType = item.OrderType;
    this.orderUpdate.orderService = item.OrderService;
    this.orderUpdate.address = item.ReceiverAddress;
    this.orderUpdate.notes = item.OrderNote;
    showModel('updateOrderModal');


  }

  updateOrder() {
    showLoader(true);
    this.dataService.updateOrder(this.orderUpdate).subscribe((p: any) => {
      showLoader(false);
      if (p.error == 0) {
        this.findMailer();
        alert('Cập nhật thành công');
        hideModel('updateOrderModal');
      } else {
        alert('Không thể cập nhật');
      }
    });
  }

}
