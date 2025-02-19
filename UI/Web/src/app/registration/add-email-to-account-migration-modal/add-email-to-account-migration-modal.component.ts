import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SafeUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { ConfirmService } from 'src/app/shared/confirm.service';
import { AccountService } from 'src/app/_services/account.service';
import { MemberService } from 'src/app/_services/member.service';
import { ServerService } from 'src/app/_services/server.service';

@Component({
  selector: 'app-add-email-to-account-migration-modal',
  templateUrl: './add-email-to-account-migration-modal.component.html',
  styleUrls: ['./add-email-to-account-migration-modal.component.scss']
})
export class AddEmailToAccountMigrationModalComponent implements OnInit {

  @Input() username!: string;
  @Input() password!: string;

  isSaving: boolean = false;
  registerForm: FormGroup = new FormGroup({});
  emailLink: string = '';
  emailLinkUrl: SafeUrl | undefined;
  error: string = '';

  constructor(private accountService: AccountService, private modal: NgbActiveModal, 
    private serverService: ServerService, private confirmService: ConfirmService, private toastr: ToastrService) {
  }

  ngOnInit(): void {
    this.registerForm.addControl('username', new FormControl(this.username, [Validators.required]));
    this.registerForm.addControl('email', new FormControl('', [Validators.required, Validators.email]));
    this.registerForm.addControl('password', new FormControl(this.password, [Validators.required]));
  }

  close() {
    this.modal.close(false);
  }

  save() {
    const model = this.registerForm.getRawValue();
      model.sendEmail = false;
      this.accountService.migrateUser(model).subscribe(async () => {
        // if (!canAccess) {
        //   // Display the email to the user
        //   this.emailLink = email;
        //   await this.confirmService.alert('Please click this link to confirm your email. You must confirm to be able to login. The link is in your logs. You may need to log out of the current account before clicking. <br/> <a href="' + this.emailLink + '" target="_blank">' + this.emailLink + '</a>');
        //   this.modal.close(true);
        // } else {
        //   await this.confirmService.alert('Please check your email (or logs under "Email Link") for the confirmation link. You must confirm to be able to login.');
        //   this.modal.close(true);
        // }
        this.toastr.success('Email has been validated');
        this.modal.close(true);
      }, err => {
        this.error = err;
      });
  }

  

}
