import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm')editForm:NgForm;
  Member:Member;
  user:User;
  @HostListener('window:beforeunload',['$event'])UnloadNotification($event:any){     //access brwsr event
    if(this.editForm.dirty){
      $event.returnValue = true;
    }
  }         

  constructor(private accountService:AccountService,private memberService:MembersService,private toastr:ToastrService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>this.user=user);
   }

  ngOnInit(): void {
    this.loadMember();
  }

loadMember(){
  this.memberService.getMember(this.user.username).subscribe(member=>{
    this.Member=member;
  })
}

updateMember(){
  this.memberService.updateMember(this.Member).subscribe(()=>
  {
  this.toastr.success("profile Update successfully");
  this.editForm.reset(this.Member);
  })
  
}
}
