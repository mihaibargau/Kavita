import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';

@Component({
  selector: 'app-badge-expander',
  templateUrl: './badge-expander.component.html',
  styleUrls: ['./badge-expander.component.scss']
})
export class BadgeExpanderComponent implements OnInit {

  @Input() items: Array<any> = [];
  @Input() itemsTillExpander: number = 4;
  @ContentChild('badgeExpanderItem') itemTemplate!: TemplateRef<any>;


  visibleItems: Array<any> = [];
  isCollapsed: boolean = false;

  get itemsLeft() {
    return Math.max(this.items.length - this.itemsTillExpander, 0);
  }
  constructor() { }

  ngOnInit(): void {
    this.visibleItems = this.items.slice(0, this.itemsTillExpander);
  }

  toggleVisible() {
    this.isCollapsed = !this.isCollapsed;

    this.visibleItems = this.items;
  }

}
