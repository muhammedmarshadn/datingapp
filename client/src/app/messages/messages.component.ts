import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/Pagination';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages:Message[] = [];
  pagination:Pagination;
  container='Unread';
  pageNumber=1;
  pageSize=5;
  loading = false;

  constructor(private messageService:MessageService,private confirmService:ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    this.loading= true;
    this.messageService.getMessages(this.pageNumber,this.pageSize,this.container).subscribe(response=>{
      this.messages = response.result;
      this.pagination = response.pagination;
      this.loading = false;
    })
  }

  deleteMessage(id:number){
    this.confirmService.confirm('confirm delete message','changes cannot be undone').subscribe(result=>{   //modal before deleting
      if(result){                                                           // if true delete msg
        this.messageService.deleteMessage(id).subscribe(()=>{
          this.messages.splice(this.messages.findIndex(m=>m.id,1));
        })
      }
    })
    
  }

  pageChanged(event:any){
    if(this.pageNumber!==event.page)
    {
    this.pageNumber = event.page;
    this.loadMessages();
    }
    
  }
}
