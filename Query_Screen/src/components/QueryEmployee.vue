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
            <button :disabled="!isCancel" @click="cancel()">取消</button>
        </div>
        <div v-if="employee" style="height: 600px">
            <table> 
                <thead>
                    <tr>
                        <th>EmployeeId</th>
                        <th>LastName</th>
                        <th>FirstName</th>
                        <!--<th>Title</th>
                        <th>TitleOfCourtesy</th>
                        <th>BirthDate</th>
                        <th>HireDate</th>
                        <th>Address</th>
                        <th>City</th>
                        <th>Region</th>
                        <th>PostalCode</th>-->
                    </tr>
                </thead>
                <tbody>
                    <tr >
                        <td><input type="text" v-model="employee.employeeId" readonly/></td>
                        <td><input type="text" v-model="employee.lastName" :readonly="isQuery"/></td>
                        <td><input type="text" v-model="employee.firstName" :readonly="isQuery"/></td>
                        <!--<td><input type="text" v-model="employee.title" /></td>
                        <td><input type="text" v-model="employee.titleOfCourtesy" /></td>
                        <td><input type="text" v-model="employee.birthDate" /></td>
                        <td><input type="text" v-model="employee.hireDate" /></td>
                        <td><input type="text" v-model="employee.address" /></td>
                        <td><input type="text" v-model="employee.city" /></td>
                        <td><input type="text" v-model="employee.region" /></td>
                        <td><input type="text" v-model="employee.postalCode" /></td>-->
                    </tr>
                </tbody>
            </table>
            <div v-if="message">{{message}}</div> 
            <div>{{employee}}</div>
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
                isCreate: false,
                isUpdate: false,
                isEdit: true,
                isQuery: true,
                isDelete: false,
                queryString: '',
                message: '',
            };
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
                        console.log(this.employee);
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
            create(){
                fetch('http://localhost:5185/api/Employee/', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.employee)
                })
                    .then(r => r.json())
                    .then(json => {
                        this.employee = json;
                        return;
                    });
            },
            edit(){
                this.isCreate = false;
                this.isUpdate = true;
                this.isEdit = false;
                this.isQuery = false;
                this.isDelete = false;
                this.isCancel = true;
            },
            cancel(){
                this.isCreate = false;
                this.isUpdate = false;
                this.isEdit = true;
                this.isQuery = true;
                this.isDelete = false;
                this.isCancel = false;
            },
            query(){
                this.isCreate = false;
                this.isUpdate = false;
                this.isEdit = true;
                this.isQuery = true;
                this.isDelete = false;
                this.isCancel = false;
            },
            
        },
    });
</script>

<style scoped>

</style>