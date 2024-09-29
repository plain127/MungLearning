using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReqandRes
{
    
    [Serializable]
    public class RequestData
    {
        public string serviceKey;
        
        // UI를 통해 입력 받는 값들
        public string bgnde;
        public string endde;
        
        public string pageNo = "1"; 
        public string upkind = "417000";  
        public string upr_cd = "6440000"; 
        public string org_cd = "4490000";
        public string state = "protect"; 
        public string neuter_yn = null; 
    }

    [Serializable]
    public class ApiResponse
    {
        public Response response;
    }

    [Serializable]
    public class Response
    {   
        public Header header;
        public Body body;
    }

    [Serializable]
    public class Header
    {
        public string reqNo;
        public string resultCode;
        public string resultMsg;
    }


    [Serializable]  
    public class Body
    {
        public ItemWrapper items; 
    }

    [Serializable]
    public class ItemWrapper
    {
        public Item[] item; 
    }

    [Serializable]
    public class Item
    {
        public string desertionNo;
        public string filename;
        public string happenDt;
        public string happenPlace;
        public string kindCd;
        public string colorCd;
        public string age;
        public string weight;
        public string sexCd;
        public string neuterYn;
        public string specialMark;
        public string noticeNo;
        public string noticeSdt;
        public string noticeEdt;
        public string popfile;
        public string processState;
        public string careNm;
        public string careTel;
        public string careAddr;
        public string orgNm;
        public string chargeNm;
        public string officetel;
    }
} 
