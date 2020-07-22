import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  public orders: Order[];
  public connectorNeeded: string;
  public currentPage: number;
  private pageSize: number;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.pageSize = 10;
    this.currentPage = 1;
  }

  ngOnInit() {
    let skip = (this.currentPage - 1) * 10;
    this.http.get<Order[]>(this.baseUrl + 'autopoco/api/samplesales/_table/orders?$orderby=Order_Id&$skip='+ skip + '&$top=' + this.pageSize + '&$expand=CustomersObjectFromCustomer_id').subscribe(results => {
      this.orders = results;

    }, error => {
        console.error(error);
        this.connectorNeeded = 'Connector sampleSales not set up or disabled.  Go to the dashboard to configure.';
    });
  }
;


  onChangePage(currentPage: number) {
    this.currentPage = currentPage;
    this.ngOnInit();
  };




}

//interface HomeViewModel {
//  connectorNeeded: string;
//  currentPage: number;
//  count: number;
//  pageSize: number;
//  totalPages: number;
//  orders: Order[];
//}
interface Order {
  order_id: number;
  customer_id: number;
  order_status: number;
  CustomersObjectFromCustomer_id: Customer;
}

interface Customer {
  last_name: string;
  first_name: string;
}
