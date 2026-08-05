import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { apiErrorMessage } from '../../core/error-message';
import { Customer, CustomerSummary } from '../../core/models';

@Component({selector:'app-customers',imports:[ReactiveFormsModule],template:`
  <section class="page customer-head"><div><span class="eyebrow">CUSTOMER DIRECTORY</span><h1>People behind the wheel.</h1><p>Keep driver details accurate and ready for their next reservation.</p></div><button class="button dark" (click)="openCreate()">＋ Add customer</button></section>
  <section class="page customer-content">
    <div class="toolbar"><label class="search"><span>⌕</span><input [formControl]="search" placeholder="Search name or email"></label><span>{{total()}} active customers</span></div>
    @if(error()){<div class="notice error">{{error()}} <button (click)="load()">Try again</button></div>}
    <div class="table-wrap"><table><thead><tr><th>Customer</th><th>Email</th><th>Status</th><th></th></tr></thead><tbody>
      @if(loading()){@for(x of skeletons;track $index){<tr><td colspan="4"><div class="skeleton row"></div></td></tr>}}
      @else{@for(customer of customers();track customer.id){<tr><td><span class="initials">{{customer.firstName.charAt(0)}}{{customer.lastName.charAt(0)}}</span><strong>{{customer.firstName}} {{customer.lastName}}</strong></td><td>{{customer.email}}</td><td><span class="active-dot">Active</span></td><td><button class="edit" (click)="openEdit(customer)">Edit →</button></td></tr>}@empty{<tr><td colspan="4" class="no-data">No customers match your search.</td></tr>}}
    </tbody></table></div>
  </section>
  @if(modalOpen()){<div class="modal-backdrop" (click)="close()"><section class="modal" (click)="$event.stopPropagation()"><button class="modal-close" (click)="close()">×</button><span class="eyebrow">CUSTOMER PROFILE</span><h2>{{editingId()?'Edit customer':'Add customer'}}</h2><form [formGroup]="form" (ngSubmit)="save()"><label>First name<input formControlName="firstName"></label><label>Last name<input formControlName="lastName"></label><label class="full">Email<input type="email" formControlName="email"></label><label class="full">Phone <small>Optional</small><input type="tel" formControlName="phone" placeholder="+1 555 0123"></label>@if(formError()){<div class="notice error full">{{formError()}}</div>}<div class="form-actions full">@if(editingId()){<button type="button" class="button danger" (click)="remove()">Delete</button>}<button type="button" class="button secondary" (click)="close()">Cancel</button><button class="button primary" [disabled]="form.invalid||saving()">{{saving()?'Saving…':'Save customer'}}</button></div></form></section></div>}
  @if(success()){<div class="toast">✓ {{success()}}</div>}
`,styleUrl:'./customers.component.scss',changeDetection:ChangeDetectionStrategy.OnPush})
export class CustomersComponent{
  private readonly api=inject(ApiService);private readonly fb=inject(FormBuilder);readonly skeletons=Array(5);readonly customers=signal<CustomerSummary[]>([]);readonly total=signal(0);readonly loading=signal(true);readonly error=signal('');readonly success=signal('');readonly modalOpen=signal(false);readonly editingId=signal<string|null>(null);readonly rowVersion=signal('');readonly saving=signal(false);readonly formError=signal('');readonly search=this.fb.nonNullable.control('');readonly form=this.fb.group({firstName:['',Validators.required],lastName:['',Validators.required],email:['',[Validators.required,Validators.email]],phone:['']});
  constructor(){this.search.valueChanges.pipe(debounceTime(300),distinctUntilChanged()).subscribe(()=>this.load());this.load()}
  load(){this.loading.set(true);this.api.customers(1,100,this.search.value).subscribe({next:r=>{this.customers.set(r.items);this.total.set(r.totalCount);this.loading.set(false)},error:e=>{this.error.set(apiErrorMessage(e));this.loading.set(false)}})}
  openCreate(){this.editingId.set(null);this.form.reset({firstName:'',lastName:'',email:'',phone:''});this.modalOpen.set(true)}
  openEdit(item:CustomerSummary){this.api.customer(item.id).subscribe({next:c=>this.populate(c),error:e=>this.error.set(apiErrorMessage(e))})}private populate(c:Customer){this.editingId.set(c.id);this.rowVersion.set(c.rowVersion);this.form.patchValue({firstName:c.firstName,lastName:c.lastName,email:c.email,phone:c.phone??''});this.modalOpen.set(true)}
  close(){this.modalOpen.set(false);this.formError.set('')}save(){if(this.form.invalid)return;this.saving.set(true);const raw=this.form.getRawValue(),input={firstName:raw.firstName!,lastName:raw.lastName!,email:raw.email!,phone:raw.phone||null,rowVersion:this.rowVersion()};const req=this.editingId()?this.api.updateCustomer(this.editingId()!,input):this.api.createCustomer(input);req.subscribe({next:()=>this.done('Customer saved successfully.'),error:e=>{this.formError.set(apiErrorMessage(e));this.saving.set(false)}})}
  remove(){if(!this.editingId()||!confirm('Delete this customer? Existing historical records are preserved.'))return;this.saving.set(true);this.api.deleteCustomer(this.editingId()!,this.rowVersion()).subscribe({next:()=>this.done('Customer removed.'),error:e=>{this.formError.set(apiErrorMessage(e));this.saving.set(false)}})}private done(message:string){this.saving.set(false);this.close();this.success.set(message);this.load();setTimeout(()=>this.success.set(''),3500)}
}
