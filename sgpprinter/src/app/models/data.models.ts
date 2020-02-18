export interface UserToken {
        access_token: string;
        token_type: string;
        expires_in: number;
        userName: string;
}


export interface MailerInfo {
        Id: string;
        MailerID: string;
        UserSend?: any;
        CustomerId: string;
        ReceiverAddress: string;
        ReceiverName: string;
        ReceiverPhone: string;
        ReceiverDistrict: string;
        OrderContent: string;
        OrderNote: string;
        OrderWeight?: any;
        OrderQuality?: any;
        OrderService: string;
        OrderType: string;
        OrderWidth?: any;
        OrderHeight?: any;
        OrderLength?: any;
        OrderCode: string;
        CreateDate: string;
        CreateTime: string;
        ModifyDate?: any;
        ModifyTime?: any;
        UserTake?: any;
        CurrentStatus: number;
        StatusNotes: string;
        Price?: any;
        ServicePrice?: any;
        ReceiverProvince: string;
        COD?: any;
        SGPStatus?: any;
        PostID:String;
}


export interface OrderStatus {
        code: number;
        name: string;
}
