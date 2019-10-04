export const APIURL = 'http://221.133.7.92:801/';
//export const APIURL = 'http://localhost:24494/';
declare var $: any;

export function encodeBase64(str: string) {

    try {
        if (typeof str == 'object') {
            throw 'error';
        }
        return window.btoa(unescape(encodeURIComponent(str)));
    } catch (e) {
        return '';
    }

}

//Decode
export function decodeBase64(str: string) {

    try {
        if (typeof str != 'string') {
            throw 'error';
        }
        return decodeURIComponent(escape(window.atob(str)));
    } catch (e) {
        return '';
    }

}


export function getToken() {
    const data = localStorage.getItem('sgpinfo');

    const infoDecode = decodeBase64(data);

    const infos = JSON.parse(infoDecode);

    return infos.access_token;
}

export function errorHandle(error: any) {
    console.log(error);
    if (error.status === 401) {
        localStorage.removeItem('sgpinfo');
        alert('Bạn cần đăng nhập lại');
    } else {
        alert(error.error.Message);
    }
}

export function showLoader(isShow) {
    if (isShow) {
        $("#loader").css("display", "block");
    } else {
        $("#loader").css("display", "none");
    }
}


export class Constants {
    static readonly DATE_FMT = 'dd/MM/yyyy';
    static readonly DATE_TIME_FMT = `${Constants.DATE_FMT} hh:mm:ss`;
  }
  