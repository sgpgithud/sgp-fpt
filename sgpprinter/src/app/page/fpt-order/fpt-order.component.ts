import { Component, OnInit } from '@angular/core';
import { DataService } from '../../service/data.service';
import { MailerInfo, OrderStatus } from '../../models/data.models';
import { errorHandle, showLoader, showModel, hideModel } from '../../utils/settings';
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
  orders: any[];
  showSynButton = 0;
  OrderStatus: OrderStatus[];
  DeliveryStatus = [{ "code": 0, "name": "Tất cả" },{ "code": 6, "name": "Đã phát" } ,{ "code": 5, "name": "Đang phát" }, { "code": 15, "name": "Chưa phát" }];
  showconfig = false;

  provinces: any[];
  districts: any[];

  orderSevices = [{ "code": "SN", "name": "Nhanh" }, { "code": "ST", "name": "Thường" }];
  orderTypes = [{ "code": "HH", "name": "Hàng hóa" }, { "code": "T", "name": "Thư" }];

  userInfos: any;

  orderUpdate = {
    id: 0,
    mailerid: "",
    provincePMS: "",
    orderType: "",
    orderService: "",
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
      mailerid: '',
      deliveryStatus: 0
    });



    /*
    this.OrderStatus = [{ "code": -1, "name": "Tất cả" }, { "code": 0, "name": "Mới tạo" },
    { "code": 1, "name": "Đã hủy" }, { "code": 2, "name": "Đã nhận" },
    { "code": 3, "name": "Đã nhận hàng" }, { "code": 4, "name": "Phát thành công" },
    { "code": 5, "name": "Phát không thành công" }, { "code": 6, "name": "Đang phát" }];
*/
    /*
        this.OrderStatus = [{ "code": 0, "name": "Mới tạo" },
        { "code": 1, "name": "Đã hủy" }, { "code": 2, "name": "Đã xác nhận" },
        { "code": 3, "name": "Đã nhận hàng" }];
    */
    this.OrderStatus = [{ "code": 0, "name": "Đơn mới từ FPT" },
    { "code": 3, "name": "Đã nhận hàng" }];

    this.interval = setInterval(() => {
      this.findMailer();
    }, 1000 * 3 * 60)

    this.findMailer();

    this.dataService.getProvince().subscribe((p: any) => {
      this.provinces = p.data;
      console.log(p.data);
    });

    this.dataService.getUserInfos().subscribe((p: any) => {
      this.userInfos = p.data;
    });

  }

  findDistrict() {
    this.dataService.getDistrict(this.orderUpdate.provincePMS).subscribe((p: any) => {
      this.districts = p.data;
    });
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
    var url =  APIURL + 'Report/PhieuGui?dateFrom=' + this.searchForm.value.dateFrom + '&dateTo='
     + this.searchForm.value.dateTo + '&status=' + this.searchForm.value.status +'&postId=' + this.userInfos.PostID;

    window.open(url, '_blank');

  }
  printOneReport(mailerid: string) {

    window.open(APIURL + 'Report/InMotPhieu?id=' + mailerid, '_blank');

  }
  printExcels() {
    var url = APIURL + 'api/fpthook/GetOrderExcel?dateFrom=' + 
    this.searchForm.value.dateFrom + '&dateTo=' + this.searchForm.value.dateTo + '&status=' + this.searchForm.value.status 
    + '&mailerid=' + this.searchForm.value.mailerid + '&postId=' + this.userInfos.PostID;
    window.open(url, '_blank');
  }

  showSearch() {
    this.showconfig = !this.showconfig;
  }

  changeStatus() {

    if (this.searchForm.value.status == 3) {
      this.showSynButton = 1;
    } else { this.showSynButton = 0; }

    this.findMailer();
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


  sendAccept(accept, orderId, idx) {

    if (confirm(accept === 1 ? 'Xác nhận đơn hàng' : 'Không nhận đơn')) {

      showLoader(true);

      this.dataService.sendAccept(accept, orderId).subscribe(p => {
        showLoader(false);
        if (p['error'] === 0) {
          this.orders.splice(idx, 1);
          if (accept === 1) {
            alert('Đã nhận đơn');
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


  showUpdateOrder(item) {
    this.orderUpdate.id = item.Id;
    this.orderUpdate.mailerid = item.OrderCode;
    this.orderUpdate.provincePMS = item.ReceiverProvincePMS;
    this.orderUpdate.orderType = item.OrderType;
    this.orderUpdate.orderService = item.OrderService;
    this.orderUpdate.address = item.ReceiverAddress;
    this.orderUpdate.weight = item.OrderWeight;
    this.orderUpdate.quantity = item.OrderQuality;
    this.orderUpdate.notes = item.OrderNote;
    showModel('updateOrderModal');
  }

  updateOrder() {
    if(this.orderUpdate.weight == 0) {
      alert('Nhập trọng lượng');
      return;
    }

    if (this.orderUpdate.quantity == 0) {
      alert('Nhập số lượng');
      return;
    }

    if (!this.orderUpdate.provincePMS) {
      alert('Nhập tỉnh thành');
      return;
    }

    showLoader(true);
    this.dataService.updateFPTOrder(this.orderUpdate).subscribe((p: any) => {
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
