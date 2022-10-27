import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { using } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import {NgxGalleryOptions} from '@kolkov/ngx-gallery'
import {NgxGalleryImage} from '@kolkov/ngx-gallery'
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit , OnDestroy {
  @ViewChild('memberTabs',{static:true}) memberTabs:TabsetComponent
  Member:Member;
  galleryOptions:NgxGalleryOptions[];
  galleryImages:NgxGalleryImage[];
  activeTab:TabDirective;
  messages:Message[]=[];
  user:User;

  constructor(public presence:PresenceService,private route:ActivatedRoute,private memberService:MembersService,private router:Router,
      private messageService:MessageService,private accountService : AccountService, private toastr: ToastrService) {

        this.accountService.currentUser$.pipe(take(1)).subscribe(user=>this.user = user);   //accessing currentUser
        this.router.routeReuseStrategy.shouldReuseRoute = ()=> false;         // preventing reuse of routing (help in routing to recipient msg when user is in another user's tab)
       }


  ngOnInit(): void {
    this.route.data.subscribe(data=>{
      this.Member = data.member;
    })

    this.route.queryParams.subscribe(params=>{
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    })

    this.galleryOptions =
    [
      {
        width:'500px',
        height: '500px',
        imagePercent:100,
        thumbnailsColumns:4,
        imageAnimation:NgxGalleryAnimation.Slide,
        preview:false
      }
    ]

    this.galleryImages=this.getImages();

  }

    getImages():NgxGalleryImage[]
    {
      const imageUrls=[];
      for(const photo of this.Member.photos){
        imageUrls.push({
          small:photo?.url,
          medium:photo?.url,
          big:photo?.url
        })
      }
      return imageUrls;
    }

 addLike(member:Member)
  {
    this.memberService.addLike(member.username).subscribe(()=>{
      this.toastr.success('You Liked'+member.knownAs);
    })
  }


loadMessages(){
  this.messageService.getMessageThread(this.Member.username).subscribe(messages =>{
    this.messages = messages;
  })
}

selectTab(tabId:number){
  this.memberTabs.tabs[tabId].active=true;
}

onTabActivated(data:TabDirective){
  this.activeTab = data;
  if(this.activeTab.heading === 'Messages' && this.messages.length ===0){
    this.messageService.createHubConnection(this.user,this.Member.username)  // creatng hubConnction open message tab
  }
  else
  {
    this.messageService.stopHubConnection();     //if user go away from message tab stop hubconnctwion
  }

}

ngOnDestroy(): void {                          // when the compont closes  stop hubcnnctn
  this.messageService.stopHubConnection();         
}
}
