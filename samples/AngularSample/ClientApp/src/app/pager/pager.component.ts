import { Component, Input, Output, EventEmitter, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-pager',
  templateUrl: './pager.component.html',
})

export class PagerComponent {
  @Output() changePage = new EventEmitter<any>(true);
  @Input() currentPage: number;

  public pages: number[];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.setPaging();
  }


  private setPage(page: number) {
    this.changePage.emit(page);
    this.setPaging();
  }

  private setPaging() {
    this.http.get<Count[]>(this.baseUrl + 'autopoco/api/samplesales/_table/orders?$count=true').subscribe(results => {
      
      let pageEnd = Math.min(results[0].count/10, this.currentPage + 10);
      this.pages = [];
      for (let i = this.currentPage; i < pageEnd; i++) {
        this.pages.push(i);
      }
    }, error => {
      console.error(error);
    });
    
  };
}

interface Count {
  count: number;
}



