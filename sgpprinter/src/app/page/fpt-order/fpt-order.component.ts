import { Component, OnInit } from '@angular/core';
import { DataService } from '../../service/data.service';
import { MailerInfo, OrderStatus } from '../../models/data.models';
import { errorHandle, showLoader } from '../../utils/settings';
import { FormBuilder, FormGroup } from '@angular/forms';
import { APIURL } from '../../utils/settings';
declare var $: any;
import { formatDate } from '@angular/common';

@Component({
  selector: 'app-fpt-order',
  templateUrl: './fpt-order.component.html',
  styleUrls: ['./fpt-order.component.css']
})
export class FptOrderComponent implements OnInit {

  constructor(private dataService: DataService, private formBuilder: FormBuilder) { }
  rootUrl = APIURL;
  searchForm;
  interval;
  orders: MailerInfo[];
  showSynButton = 0;
  OrderStatus: OrderStatus[];
  DeliveryStatus = [{ "code": 0, "name": "Tất cả" }, { "code": 6, "name": "Đang phát" }, { "code": 13, "name": "Chưa phát được" }];
  showconfig = false;
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
      mailerid: '',
      deliveryStatus: 0
    });



    /*
    this.OrderStatus = [{ "code": -1, "name": "Tất cả" }, { "code": 0, "name": "Mới tạo" },
    { "code": 1, "name": "Đã hủy" }, { "code": 2, "name": "Đã nhận" },
    { "code": 3, "name": "Đã nhận hàng" }, { "code": 4, "name": "Phát thành công" },
    { "code": 5, "name": "Phát không thành công" }];
*/
    /*
        this.OrderStatus = [{ "code": 0, "name": "Mới tạo" },
        { "code": 1, "name": "Đã hủy" }, { "code": 2, "name": "Đã xác nhận" },
        { "code": 3, "name": "Đã nhận hàng" }];
    */
    this.OrderStatus = [{ "code": 0, "name": "Đơn mới từ FPT" }, { "code": 2, "name": "Đã gửi tới mobile" },
    { "code": 3, "name": "Đã nhận hàng" }];

    this.interval = setInterval(() => {
      this.findMailer();
    },1000*3*60)

    this.findMailer();

  }

  findMailer() {

    showLoader(true);
    this.dataService.getFPTOrder(this.searchForm.value).subscribe((data: any) => {
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
  printOneReport(mailerid: string) {

    window.open(this.dataService.printOneReports(this.searchForm.value, mailerid), '_blank');

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
    window.open(this.dataService.printExcels(this.searchForm.value), '_blank');

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

  synData() {
    if (confirm('Đồng bộ dữ liệu mobile')) {
      showLoader(true);
      this.dataService.sendSynData().subscribe(p => {
        showLoader(false);
        if (p['error'] === 0) {
          alert('Thành công');
          this.findMailer();
        } else {
          alert('Không thể đồng bộ');
        }
      },
        error => {
          showLoader(false);
          errorHandle(error);
        });

    }
  }

  synPMS() {
    if (confirm('Đồng bộ dữ liệu pms')) {
      showLoader(true);
      this.dataService.sendSynPMSData().subscribe(p => {
        showLoader(false);
        if (p['error'] === 0) {
          alert('Thành công');
          this.findMailer();
        } else {
          alert('Không thể đồng bộ');
        }
      },
        error => {
          showLoader(false);
          errorHandle(error);
        });

    }
  }


  acceptAll() {
    if (confirm('Xac nhan toan bo')) {

      for(var idx = 0; idx < this.orders.length;idx++) {
        showLoader(true);
        this.dataService.sendAccept(1, this.orders[idx].Id).subscribe(p => {
          showLoader(false);
          if (p['error'] === 0) {
            this.sendToPhu(p['data']);
          } 
        },
          error => {
            showLoader(false);
            errorHandle(error);
          });
      }
    }
  }

  sendAccept(accept, orderId, idx) {

    if (confirm(accept === 1 ? 'Xác nhận đơn hàng' : 'Không nhận đơn')) {

      showLoader(true);

      this.dataService.sendAccept(accept, orderId).subscribe(p => {
        showLoader(false);
        if (p['error'] === 0) {
          this.orders.splice(idx, 1);
          if (accept === 1) {
            this.sendToPhu(p['data']);
          } else {
            alert('Đã hủy đơn');
          }
        } else {
          alert('Không thể xử lý đơn này');
        }
      },
        error => {
          showLoader(false);
          errorHandle(error);
        });
    }
  }


  sendToPhu(data) {
    showLoader(true);
    this.dataService.sendToPhu(data).subscribe(p => {
      console.log(p);
      showLoader(false);
      alert('Thành công');
    });
  }
}
