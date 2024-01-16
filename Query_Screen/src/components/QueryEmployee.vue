<template>
    <div class="weather-component">
        <h1>Query Employee</h1>
        <div v-if="loading" class="loading">
            Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationvue">https://aka.ms/jspsintegrationvue</a> for more details.
        </div>
        <div>
            <input type="text" v-model="queryString" placeholder="請輸入Id" :disabled="!isEdit" />
            <button :disabled="!isQuery" @click="search">查詢</button>
            <button :disabled="!isCreate" @click="create">新增</button>
            <button :disabled="!isEdit" @click="edit()">修改</button>
            <button :disabled="!isUpdate" @click="update(item)">更新</button>
            <button :disabled="!isDelete" @click="deleteItem(item.id)">刪除</button>
            <button :disabled="!isSave" @click="add(item)">儲存</button>
            <button :disabled="!isCancel" @click="cancel()">取消</button>
        </div>
        <div v-if="employee" style="height: 600px">
            <div class="panel">
                <div class="panel-row">
                    <div panel-header>EmployeeId</div>
                    <div panel-content><input type="text" v-model="employee.employeeId" readonly/></div>
                    <div panel-header>LastName</div>
                    <div panel-content><input type="text" v-model="employee.lastName" :readonly="isQuery"/></div>
                    <div panel-header>FirstName</div>
                    <div panel-content><input type="text" v-model="employee.firstName" :readonly="isQuery"/></div>
                </div>
                <div class="panel-row">
                    <div panel-header>Title</div>
                    <div panel-content><input type="text" v-model="employee.title" :readonly="isQuery"/></div>
                    <div panel-header>TitleOfCourtesy</div>
                    <div panel-content><input type="text" v-model="employee.titleOfCourtesy" :readonly="isQuery"/></div>
                    <div panel-header>BirthDate</div>
                    <div panel-content><input type="text" v-model="employee.birthDate" :readonly="isQuery"/></div>
                </div>   
                <div class="panel-row">
                    <div panel-header>HireDate</div>
                    <div panel-content><input type="text" v-model="employee.hireDate" :readonly="isQuery"/></div>
                    <div panel-header>Address</div>
                    <div panel-content><input type="text" v-model="employee.address" :readonly="isQuery"/></div>
                    <div panel-header>City</div>
                    <div panel-content><input type="text" v-model="employee.city" :readonly="isQuery"/></div>
                </div>
                <div class="panel-row">
                    <div panel-header>Region</div>
                    <div panel-content><input type="text" v-model="employee.region" :readonly="isQuery"/></div>
                    <div panel-header>PostalCode</div>
                    <div panel-content><input type="text" v-model="employee.postalCode" :readonly="isQuery"/></div>
                    <div panel-header>Country</div>
                    <div panel-content><input type="text" v-model="employee.country" :readonly="isQuery"/></div>
                </div>
                <div class="panel-row">
                    <div panel-header>HomePhone</div>
                    <div panel-content><input type="text" v-model="employee.homePhone" :readonly="isQuery"/></div>
                    <div panel-header>Extension</div>
                    <div panel-content><input type="text" v-model="employee.extension" :readonly="isQuery"/></div>
                    <div panel-header>Photo</div>
                    <div panel-content>
                        <div><img :src="imageSrc" ref="profileImg" alt="" srcset=""></div>
                        <div><input id="imgReader" ref="fileUploader" type="file" @change="imageLoad" :style="{display:!isCreate?'initial':'none'}"/></div>
                    </div>
                </div>
                <div class="panel-row">
                    <div panel-header>Notes</div>
                    <div panel-content><input type="text" v-model="employee.notes" :readonly="isQuery"/></div>
                    <div panel-header>ReportsTo</div>
                    <div panel-content><input type="text" v-model="employee.reportsTo" :readonly="isQuery"/></div>
                    <div panel-header>PhotoPath</div>
                    <div panel-content><input type="text" v-model="employee.photoPath" :readonly="isQuery"/></div>
                </div>
            </div>
            
            
            <div v-if="message">{{message}}</div> 
            
        </div>
    </div>
</template>

<script lang="js">
    import { defineComponent } from 'vue';

    export default defineComponent({
        data() {
            return {
                loading: false,
                employee: null,
                isCreate: true,
                isUpdate: false,
                isEdit: true,
                isQuery: true,
                isSave: false,
                isDelete: false,
                queryString: '',
                message: '',
                imgReduce: 'FRwvAAIAAAANAA4AFAAhAP////9CaXRtYXAgSW1hZ2UAUGFpbnQuUGljdHVyZQABBQAAAgAAAAcAAABQQnJ1c2gAAAAAAAAAAAAgVAAA',
            };
        },
        computed:{
            imageSrc:{
                get(){
                    if(this.employee.photo){
                        return 'data:image/jpeg;base64,' + this.employee.photo.substr(104);
                    }
                    return '';
                }
            }
        },
        created() {
            // fetch the data when the view is created and the data is
            // already being observed
            this.fetchData(); 
        },
        watch: {
            // call again the method if the route changes
            '$route': 'fetchData'
        },
        methods: {
            fetchData() {
                this.employee = null;
                this.loading = true;

                fetch('http://localhost:5185/api/Employee/1') 
                    .then(r => r.json()) 
                    .then(json => {
                        this.employee = json;
                        this.loading = false;
                        return;
                    });
            },
            search() {
                this.employee = null;
                fetch('http://localhost:5185/api/Employee/' + this.queryString) 
                    .then(r => r.json()) 
                    .then(json => {
                        this.employee = json;
                        return;
                    });
            },
            update(){
                fetch('http://localhost:5185/api/Employee/' + this.queryString, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.employee)
                })
                    .then(r =>{ 
                        this.message = r.statusText;
                        this.query();
                    })
            },
            add(){
                if(this.employee.firstName == '' || this.employee.lastName == ''){
                    alert('姓名不得為空！');
                    return;
                }
                let body = Object.keys(this.employee).reduce((result, key) => {
                if (this.employee[key] !== null && this.employee[key] !== '') {
                    result[key] = this.employee[key];
                }
                return result;
            }, {});
                console.log(JSON.stringify(this.employee));
                fetch('http://localhost:5185/api/Employee/', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(body)
                })
                    .then(r => r.json())
                    .then(json => {
                        this.employee = json;
                        return;
                    }).catch(e => {
                        this.message = e;
                    })
            },
            edit(){
                this.isCreate = false;
                this.isUpdate = true;
                this.isEdit = false;
                this.isQuery = false;
                this.isDelete = false;
                this.isSave = false;
                this.isCancel = true;
            },
            cancel(){
                this.isCreate = true;
                this.isUpdate = false;
                this.isEdit = true;
                this.isQuery = true;
                this.isDelete = false;
                this.isSave = false;
                this.isCancel = false;
                this.clear();
                
            },
            query(){
                this.isCreate = true;
                this.isUpdate = false;
                this.isEdit = true;
                this.isQuery = true;
                this.isDelete = false;
                this.isSave = false;
                this.isCancel = false;
            },
            create(){
                this.isCreate = false;
                this.isUpdate = false;
                this.isEdit = false;
                this.isQuery = false;
                this.isDelete = false;
                this.isSave = true;
                this.isCancel = true;
                this.queryString = '';
                this.clear();
                this.$refs.fileUploader.value = '';
            },
            clear(){
                for(var key in this.employee){
                    this.employee[key] = '';
                }
            },
            imageLoad($event){
                let file = $event.target.files[0];
                let reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = (event) => {
                    let base64String = event.target.result;
                    // this.$refs.profileImg.src = base64String;
                    // console.log(base64String.split(','));
                    this.employee.photo = this.imgReduce + base64String.split(',')[1];
                }
            }
            
        },
    });
</script>

<style scoped>
.panel{
    display: flex;
    flex-direction: column;
    justify-content: start;
    align-items: start;
    background-color: #2196F3;
    color: white;
    padding: 10px;
    margin-bottom: 10px;
}
.panel-row{
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: start;
    background-color: #7e7a7a;
    color: white;
    padding: 10px;
    margin-bottom: 10px;
    border: 1px solid #bbb;
    width: 1200px;
}
.panel-header{
   flex-basis: 200px;
}
.panel-content{
    flex-basis: 100px;
}
</style>