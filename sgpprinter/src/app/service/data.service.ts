import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { getToken, APIURL } from '../utils/settings';

@Injectable({
    providedIn: 'root',
})
export class DataService {

    constructor(private httpClient: HttpClient) {

    }

    getFPTOrder(data) {
        const token = 'bearer ' + getToken();
        const httpOption = {
            headers: new HttpHeaders({
                'Authorization': token
            })
        };

        return this.httpClient.get(APIURL + 'api/fpthook/GetOrderFPTByStatus?mailerid=' + data.mailerid + '&dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status + '&deliveryStatus=' + data.deliveryStatus, httpOption)
    }


    downloadFile(data: any, type: string) {
        const blob = new Blob([data], { type: type });
        const url = window.URL.createObjectURL(blob);
        window.open(url);
    }

    printReports(data) {
        /*
        const token = 'bearer ' + getToken();
        const httpOption = {
            responseType: 'arraybuffer' as 'arraybuffer',
            headers: new HttpHeaders({
                'Authorization': token
            })
        };

        return this.httpClient.get(APIURL + 'api/Report/PhieuGui?dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status, httpOption);

        */
        return APIURL + 'api/Report/PhieuGui?dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status;
    }
    printOneReports(data, mailerid) {
        /*
        const token = 'bearer ' + getToken();
        const httpOption = {
            responseType: 'arraybuffer' as 'arraybuffer',
            headers: new HttpHeaders({
                'Authorization': token
            })
        };

        return this.httpClient.get(APIURL + 'api/Report/PhieuGui?dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status + '&mailerid=' + mailerid, httpOption);
*/
        return APIURL + 'api/Report/InMotPhieu?mailerid=' + mailerid;
    }
    printExcels(data) {
        const token = 'bearer ' + getToken();
        const httpOption = {
            responseType: 'arraybuffer' as 'arraybuffer',
            headers: new HttpHeaders({
                'Authorization': token
            })
        };
        return APIURL + 'api/fpthook/GetOrderExcel?dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status + '&mailerid=' +  data.mailerid ;
       // return this.httpClient.get(APIURL + 'api/fpthook/GetOrderExcel?dateFrom=' + data.dateFrom + '&dateTo=' + data.dateTo + '&status=' + data.status, httpOption);
    }

    sendAccept(accept, orderId) {
        const token = 'bearer ' + getToken();
        const httpOption = {
            headers: new HttpHeaders({
                'Authorization': token
            })
        };
        return this.httpClient.get(APIURL + 'api/fpthook/AcceptOrder?isAccept=' + accept + '&orderId=' + orderId, httpOption);
    }

    sendToPhu(data) {
        var url = 'https://script.google.com/macros/s/AKfycbznjh0K1YLiAiEG3xZyGMLkpYKAo2l_MeV78h354vq2q82cyUA/exec?';

        url = url + 'SO_CG=&';
        url = url + 'FPT_Order=' + data.OrderCode + '&';
        url = url + 'NGUOI_GUI=' + data.CustomerId + '&';
        url = url + 'DC_NGUOI_GUI=&';
        url = url + 'DT_NGUOI_GUI=&';
        url = url + 'TINH_THANH_GUI=&';
        url = url + 'NGUOI_NHAN=' + data.ReceiverName + '&';
        url = url + 'DC_NHAN=' + data.ReceiverAddress + '&';
        url = url + 'DT_NHAN=&';
        url = url + 'TINH_THANH_NHAN=&';
        url = url + 'QUAN_HUYEN_NHAN=&';
        url = url + 'DICH_VU=&';
        url = url + 'HINH_THUC_THANH_TOAN=&';
        url = url + 'LOAI_HANG=&';
        url = url + 'COD=&';
        url = url + 'TRONG_LUONG=&';
        url = url + 'SO_LUONG=&';
        url = url + 'GHI_CHU=&';
        url = url + 'THONG_TIN_DON_HANG=&';
        url = url + 'Status=Đã xác nhận';

        return this.httpClient.get(encodeURI(url));
    }

    sendSynData() {
        const token = 'bearer ' + getToken();
        const httpOption = {
            headers: new HttpHeaders({
                Authorization: token
            })
        };
        return this.httpClient.get(APIURL + 'api/fpthook/SynData', httpOption);
    }


    sendSynPMSData() {
        const token = 'bearer ' + getToken();
        const httpOption = {
            headers: new HttpHeaders({
                'Authorization': token
            })
        };
        return this.httpClient.get(APIURL + 'api/fpthook/SynPMS', httpOption);
    }

}